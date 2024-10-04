import Cinema from './Cinema';
import EventDataResult from './EventDataResult';

export interface FilterServiceEvents {
  databaseReady: (dataVersionDate: Date) => void;
  eventDataReady: (data: EventDataResult) => void;
  cinemaDataReady: (data: Cinema[]) => void;
}
