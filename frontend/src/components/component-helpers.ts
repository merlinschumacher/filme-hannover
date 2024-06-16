export function SlotSpanFactory(content: string, name: string) {
  const span = document.createElement('span');
  span.slot = name;
  span.textContent = content;
  return span;
}

