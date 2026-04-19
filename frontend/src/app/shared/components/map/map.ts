import { Component, OnInit, output, input } from '@angular/core';
import * as L from 'leaflet';

// Fix leaflet default marker icon
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
export class Map implements OnInit {
  lat = input<number>(43.8563);
  lng = input<number>(18.4131);
  zoom = input<number>(6);
  
  locationSelected = output<{ lat: number, lng: number }>();

  private map!: L.Map;
  private marker: L.Marker | null = null;

  ngOnInit() {
    this.initMap();
  }

  private initMap() {
    this.map = L.map('map').setView([this.lat(), this.lng()], this.zoom());

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      this.placeMarker(e.latlng);
      this.locationSelected.emit({ lat: e.latlng.lat, lng: e.latlng.lng });
    });
  }

  private placeMarker(latlng: L.LatLng) {
    if (this.marker) {
      this.marker.remove();
    }
    this.marker = L.marker(latlng).addTo(this.map);
  }
}