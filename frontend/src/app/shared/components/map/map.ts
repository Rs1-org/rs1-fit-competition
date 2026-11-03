import { Component, AfterViewInit, OnDestroy, ViewChild, ElementRef, output, input, effect } from '@angular/core';
import * as L from 'leaflet';

/** Pin in the theme colour; styles.css drops it in when placed (skipped for reduced motion). */
export const pinIcon = L.divIcon({
  className: 'map-pin',
  html: '<svg viewBox="0 0 28 40" width="28" height="40" aria-hidden="true">'
    + '<path d="M14 1C6.8 1 1 6.7 1 13.9 1 23.6 14 39 14 39s13-15.4 13-25.1C27 6.7 21.2 1 14 1z"/>'
    + '<circle cx="14" cy="14" r="5"/></svg>',
  iconSize: [28, 40],
  iconAnchor: [14, 39],
  popupAnchor: [0, -36],
  tooltipAnchor: [0, -38],
});

L.Marker.prototype.options.icon = pinIcon;

@Component({
  selector: 'app-map',
  templateUrl: './map.html',
  styleUrl: './map.css'
})
export class Map implements AfterViewInit, OnDestroy {
  lat = input<number>(43.8563);
  lng = input<number>(18.4131);
  zoom = input<number>(6);
  markerLat = input<number | null>(null);
  markerLng = input<number | null>(null);
  // Optional name shown above the marker, e.g. the stadium.
  markerLabel = input<string>('');
  // Optional second point (e.g. city centre); when set, it is drawn and joined to the marker with a line.
  secondMarkerLat = input<number | null>(null);
  secondMarkerLng = input<number | null>(null);
  secondMarkerLabel = input<string>('');
  // Display-only maps ignore clicks so the marker cannot be moved.
  readonly = input<boolean>(false);

  locationSelected = output<{ lat: number, lng: number }>();

  @ViewChild('mapContainer') mapContainer!: ElementRef<HTMLDivElement>;

  private map!: L.Map;
  private marker: L.Marker | null = null;
  private secondMarker: L.CircleMarker | null = null;
  private line: L.Polyline | null = null;

  constructor() {
    // Keep the marker in sync when the bound coordinates change after the map is created.
    effect(() => {
      const mLat = this.markerLat();
      const mLng = this.markerLng();
      if (!this.map) return;

      if (mLat === null || mLng === null) {
        this.marker?.remove();
        this.marker = null;
        return;
      }

      const current = this.marker?.getLatLng();
      if (!current || current.lat !== mLat || current.lng !== mLng) {
        const point = L.latLng(mLat, mLng);
        this.placeMarker(point);
        // Typed coordinates can be off screen; bring the pin into view.
        if (!this.map.getBounds().contains(point)) this.map.panTo(point);
      }
    });

    // Redraw the second point and the connecting line when either point changes.
    effect(() => {
      this.markerLat();
      this.markerLng();
      this.secondMarkerLat();
      this.secondMarkerLng();
      this.secondMarkerLabel();
      if (this.map) this.drawSecondMarker();
    });
  }

  ngAfterViewInit() {
    requestAnimationFrame(() => setTimeout(() => this.initMap(), 0));
  }

  private initMap() {
    this.map = L.map(this.mapContainer.nativeElement).setView([this.lat(), this.lng()], this.zoom());

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);

    const mLat = this.markerLat();
    const mLng = this.markerLng();
    if (mLat !== null && mLng !== null) {
      this.placeMarker(L.latLng(mLat, mLng));
    }

    this.drawSecondMarker();

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      if (this.readonly()) return;
      this.placeMarker(e.latlng);
      this.locationSelected.emit({ lat: e.latlng.lat, lng: e.latlng.lng });
    });
  }

  ngOnDestroy() {
    this.map?.remove();
  }

  private placeMarker(latlng: L.LatLng) {
    if (this.marker) {
      this.marker.remove();
    }
    this.marker = L.marker(latlng, { title: this.markerLabel() }).addTo(this.map);
    if (this.markerLabel()) {
      this.marker.bindTooltip(this.markerLabel(), { direction: 'top', permanent: true });
    }
  }

  private drawSecondMarker() {
    this.secondMarker?.remove();
    this.line?.remove();
    this.secondMarker = null;
    this.line = null;

    const sLat = this.secondMarkerLat();
    const sLng = this.secondMarkerLng();
    const mLat = this.markerLat();
    const mLng = this.markerLng();
    if (sLat === null || sLng === null || mLat === null || mLng === null) return;

    const from = L.latLng(mLat, mLng);
    const to = L.latLng(sLat, sLng);

    const css = getComputedStyle(document.documentElement);
    const marker = css.getPropertyValue('--route-a').trim();
    const route = css.getPropertyValue('--route-b').trim();
    this.secondMarker = L.circleMarker(to, { radius: 8, color: marker, fillColor: marker, fillOpacity: 0.8 })
      .addTo(this.map);
    if (this.secondMarkerLabel()) {
      this.secondMarker.bindTooltip(this.secondMarkerLabel());
    }

    this.line = L.polyline([from, to], { color: route, weight: 3, dashArray: '6 6' }).addTo(this.map);
    this.map.fitBounds(L.latLngBounds(from, to), { padding: [40, 40], maxZoom: 15 });
  }
}
