import Cinema from './Cinema';
import EventDataResult from './EventDataResult';

export interface FilterServiceEvents {
  eventDataRady: (data: EventDataResult) => void;
  cinemaDataReady: (data: Cinema[]) => void;
}
