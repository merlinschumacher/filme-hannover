import Cinema from './Cinema';
import EventDataResult from './EventDataResult';

export default interface FilterServiceEvents {
  databaseReady: (dataVersionDate: Date) => void;
  cinemaDataReady: (data: Cinema[]) => void;
  dataReady: () => void;
  eventDataReady: (data: EventDataResult) => void;
}
