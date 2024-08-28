export default class HttpClient {
  protected constructor() { }

  static async getData(url: string) {
    try {
      const response = await fetch(url);
      if (!response.ok) throw response.statusText;
      return response;
    } catch (e) {
      console.error(e);
      return null;
    }
  }

  static async getJsonData(url: string) {
    try {
      const response = await this.getData(url);
      if (!response) return null;
      return await response.json();
    } catch (e) {
      console.error(e);
      return null;
    }
  }
}
