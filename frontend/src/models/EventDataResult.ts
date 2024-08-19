import { EventData } from './EventData';

export default class EventDataResult {
  constructor(eventData: Map<Date, EventData[]>, lastDate: Date) {
    this.EventData = eventData;
    this.LastShowTimeDate = lastDate ;
  }
  public EventData: Map<Date, EventData[]>
  public LastShowTimeDate: Date
}
