import Cinema from '../models/Cinema';
import EventDataResult from '../models/EventDataResult';
import FilterSelection from '../models/FilterSelection';
import Movie from '../models/Movie';
import { createNanoEvents, Emitter } from 'nanoevents';
import FilterServiceEvents from '../models/FilterServiceEvents';

export default class FilterServiceWorkerAdapter {
  private worker: Worker;
  private emitter: Emitter;
  private messageHandlers = new Map<string, (data: any) => void>();

  constructor() {
    this.worker = new Worker(new URL('../workers/FilterWorker.ts', import.meta.url), { type: 'module' });

    this.emitter = createNanoEvents<FilterServiceEvents>();
    this.worker.onmessage = this.handleWorkerMessage.bind(this);
  }

  private handleWorkerMessage(event: MessageEvent) {
    const { type, data, requestId } = event.data;

    // Handle promise resolutions for async calls
    if (requestId && this.messageHandlers.has(requestId)) {
      const handler = this.messageHandlers.get(requestId);
      if (handler) {
        handler(data);
      }
      this.messageHandlers.delete(requestId);
      return;
    }

    // Handle event emissions
    switch (type) {
      case 'databaseReady':
        this.emitter.emit('databaseReady', new Date(data));
        break;

      case 'cinemaDataReady':
        this.emitter.emit('cinemaDataReady', data);
        break;

      case 'dataReady':
        this.emitter.emit('dataReady');
        break;

      case 'eventDataReady':
        // Convert date strings back to Date objects
        if (data.EventData) {
          const convertedMap = new Map<Date, any[]>();
          Object.entries(data.EventData).forEach(([dateStr, events]) => {
            convertedMap.set(new Date(dateStr), events as any[]);
          });
          data.EventData = convertedMap;
        }
        this.emitter.emit('eventDataReady', data as EventDataResult);
        break;
    }
  }

  on<E extends keyof FilterServiceEvents>(event: E, callback: FilterServiceEvents[E]) {
    return this.emitter.on(event, callback);
  }

  // Helper method for async requests
  private async request<T>(type: string, data?: any): Promise<T> {
    return new Promise<T>((resolve) => {
      const requestId = Date.now().toString() + Math.random().toString(36).substr(2, 5);
      this.messageHandlers.set(requestId, resolve);
      this.worker.postMessage({ type, data, requestId });
    });
  }

  // Simple post methods (void return)
  private post(type: string, data?: any): void {
    this.worker.postMessage({ type, data });
  }

  // Public API methods
  loadData(): void {
    this.post('loadData');
  }

  setDateRange(startDate: Date, visibleDays: number): void {
    this.post('setDateRange', { startDate: startDate.toISOString(), visibleDays });
  }

  async getMovies(): Promise<Movie[]> {
    return this.request<Movie[]>('getMovies');
  }

  async getMovieCount(): Promise<number> {
    return this.request<number>('getMovieCount');
  }

  async getCinemas(): Promise<Cinema[]> {
    return this.request<Cinema[]>('getCinemas');
  }

  async getCinemaCount(): Promise<number> {
    return this.request<number>('getCinemaCount');
  }

  async setSelection(selection: FilterSelection): Promise<void> {
    this.post('setSelection', selection);
  }

  async getSelection(): Promise<FilterSelection> {
    return this.request<FilterSelection>('getSelection');
  }

  async getNextPage(): Promise<void> {
    this.post('getNextPage');
  }

  async getDataVersion(): Promise<string> {
    return this.request<string>('getDataVersion');
  }
}
