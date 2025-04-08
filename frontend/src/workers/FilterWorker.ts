import FilterService from '../services/FilterService';

// Create FilterService instance in the worker
const filterService = new FilterService();

// Add event listeners to forward events to main thread
const events = ['databaseReady', 'cinemaDataReady', 'dataReady', 'eventDataReady'];
events.forEach(event => {
  filterService.on(event as any, (data: any) => {
    // For EventDataResult, we need to convert the Map to a serializable object
    if (event === 'eventDataReady' && data?.EventData instanceof Map) {
      const serializedMap: Record<string, any> = {};
      data.EventData.forEach((events: any, date: Date) => {
        serializedMap[date.toISOString()] = events;
      });
      data = { ...data, EventData: serializedMap };
    }

    self.postMessage({ type: event, data });
  });
});

// Message handler with async handling
self.onmessage = async (event) => {
  const { type, data, requestId } = event.data;

  try {
    let result;

    switch (type) {
      // Void methods
      case 'loadData':
        filterService.loadData();
        break;

      case 'setDateRange':
        filterService.setDateRange(new Date(data.startDate), data.visibleDays);
        break;

      case 'setSelection':
        await filterService.setSelection(data);
        break;

      case 'getNextPage':
        await filterService.getNextPage();
        break;

      // Methods that return data
      case 'getMovies':
        result = await filterService.getMovies();
        break;

      case 'getMovieCount':
        result = await filterService.getMovieCount();
        break;

      case 'getCinemas':
        result = filterService.getCinemas();
        break;

      case 'getCinemaCount':
        result = await filterService.getCinemaCount();
        break;

      case 'getSelection':
        result = filterService.getSelection();
        break;

      case 'getDataVersion':
        result = filterService.getDataVersion();
        break;

      default:
        console.error('Unknown message type:', type);
    }

    // Only send response for requests that expect a result
    if (requestId) {
      self.postMessage({ type, data: result, requestId });
    }
  } catch (error) {
    console.error(`Error in worker handling ${type}:`, error);
    if (requestId) {
      const errorMessage = error instanceof Error ? error.message : 'An unknown error occurred';
      self.postMessage({ type: 'error', data: errorMessage, requestId });
    }
  }
};
