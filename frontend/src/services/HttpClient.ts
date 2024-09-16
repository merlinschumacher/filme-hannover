export async function getData(url: string): Promise<Response | null> {
  try {
    const response = await fetch(url);
    if (!response.ok) throw new Error(response.statusText);
    return response;
  } catch (e) {
    console.error(e);
    return null;
  }
}

export async function getJsonData(url: string): Promise<unknown> {
  try {
    const response = await getData(url);
    if (!response) return null;
    return await response.json();
  } catch (e) {
    console.error(e);
    return null;
  }
}
