import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ImageZoom } from './image-zoom';

describe('ImageZoom', () => {
  let fixture: ComponentFixture<ImageZoom>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ImageZoom] }).compileComponents();
    fixture = TestBed.createComponent(ImageZoom);
    fixture.componentRef.setInput('src', 'data:image/gif;base64,xx');
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();
  });

  it('zooms in and out and closes on Escape', () => {
    const zoom = fixture.componentInstance;
    zoom.zoom(0.25);
    expect(zoom.scale()).toBe(1.25);
    zoom.zoom(-1);
    expect(zoom.scale()).toBe(1);
    const closed = vi.fn();
    zoom.closed.subscribe(closed);
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    expect(closed).toHaveBeenCalled();
  });
});
