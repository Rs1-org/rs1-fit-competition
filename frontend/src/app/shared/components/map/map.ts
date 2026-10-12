import { Component, AfterViewInit, OnDestroy, ViewChild, ElementRef, output, input, effect } from '@angular/core';
import * as L from 'leaflet';

const iconDefault = L.icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
});

L.Marker.prototype.options.icon = iconDefault;

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
        this.placeMarker(L.latLng(mLat, mLng));
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
    this.marker = L.marker(latlng).addTo(this.map);
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

    this.secondMarker = L.circleMarker(to, { radius: 8, color: '#dc2626', fillColor: '#dc2626', fillOpacity: 0.8 })
      .addTo(this.map);
    if (this.secondMarkerLabel()) {
      this.secondMarker.bindTooltip(this.secondMarkerLabel());
    }

    this.line = L.polyline([from, to], { color: '#2563eb', weight: 3, dashArray: '6 6' }).addTo(this.map);
    this.map.fitBounds(L.latLngBounds(from, to), { padding: [40, 40], maxZoom: 15 });
  }
}
