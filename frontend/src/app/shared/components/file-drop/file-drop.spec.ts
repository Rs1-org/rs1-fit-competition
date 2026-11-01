import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FileDrop } from './file-drop';

describe('FileDrop', () => {
  let fixture: ComponentFixture<FileDrop>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [FileDrop] }).compileComponents();
    fixture = TestBed.createComponent(FileDrop);
    fixture.detectChanges();
  });

  it('holds a dropped image until Upload is clicked', () => {
    const chosen = vi.fn();
    fixture.componentInstance.chosen.subscribe(chosen);
    const file = new File(['png'], 'photo.png', { type: 'image/png' });
    fixture.componentInstance.take(file);
    expect(fixture.componentInstance.pending()).toBe(file);
    expect(fixture.componentInstance.shown()).toContain('blob:');
    expect(chosen).not.toHaveBeenCalled();
    fixture.componentInstance.upload();
    expect(chosen).toHaveBeenCalledWith(file);
  });

  it('rejects the wrong type and an oversized file', () => {
    const drop = fixture.componentInstance;
    drop.take(new File(['gif'], 'anim.gif', { type: 'image/gif' }));
    expect(drop.message()).toBe('Use a JPEG, PNG or WebP image.');
    const huge = new File(['x'], 'big.png', { type: 'image/png' });
    Object.defineProperty(huge, 'size', { value: 9 * 1024 * 1024 });
    drop.take(huge);
    expect(drop.message()).toBe('Image must be 8 MB or smaller.');
  });
});
