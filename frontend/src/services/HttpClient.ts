export default class HttpClient {
  private constructor() {
    throw new Error('Cannot instantiate HttpClient');
  }

  static async getData(url: string) {
    try {
      const response = await fetch(url);
      if (!response.ok) throw new Error(response.statusText);
      return response;
    } catch (e) {
      console.error(e);
      return null;
    }
  }

  static async getJsonData(url: string): Promise<unknown> {
    try {
      const response = await HttpClient.getData(url);
      if (!response) return null;
      return await response.json();
    } catch (e) {
      console.error(e);
      return null;
    }
  }
}
