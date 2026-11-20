import { routes } from '../app.routes';

describe('feature NgModule routing', () => {
  it('lazy-loads selection, persons and gallery modules from the root router', () => {
    const lazyChunks = routes
      .filter((route) => route.loadChildren)
      .map((route) => route.loadChildren!.toString());

    expect(lazyChunks.some((source) => source.includes('SelectionModule'))).toBe(true);
    expect(lazyChunks.some((source) => source.includes('PersonsModule'))).toBe(true);
    expect(lazyChunks.some((source) => source.includes('GalleryModule'))).toBe(true);
  });
});