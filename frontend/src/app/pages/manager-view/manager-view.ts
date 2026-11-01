import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  hasManagerHistoryData,
  ManagerHistoryForm,
} from '../../core/models/history.models';
import { pictureToDataUrl } from '../../core/models/person.models';
import { HistoryService } from '../../core/services/history.service';
import { ManagerService } from '../../core/services/manager.service';
import { fadeSwap } from '../../core/animations/motion';
import { apiErrorMessage } from '../../core/utils/api-error';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';
import { Breadcrumb } from '../../shared/components/breadcrumb/breadcrumb';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { ImageZoom } from '../../shared/components/image-zoom/image-zoom';

@Component({
  selector: 'app-manager-view',
  imports: [CommonModule, RouterModule, AppDatePipe, Breadcrumb, StatTile, Skeleton, ErrorBanner, ImageZoom],
  templateUrl: './manager-view.html',
  styleUrls: ['./manager-view.css', '../player-view/person-profile.css'],
  animations: [fadeSwap],
})
export class ManagerView implements OnInit {
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  private historyService = inject(HistoryService);
  private managerService = inject(ManagerService);

  selectedManager: Record<string, unknown> | null = null;
  pictureUrl: string | null = null;
  history: ManagerHistoryForm | null = null;
  historyRows: { label: string; value: unknown }[] = [];
  activeTab: 'info' | 'history' = 'info';
  showHistoryTab = false;
  loading = true;
  error = '';
  zoom = false;
  private managerId = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('publicId');
    if (!id) {
      this.loading = false;
      this.error = 'Manager could not be loaded.';
      return;
    }
    this.managerId = id;
    this.loadManager(id);
  }

  retry(): void {
    if (this.managerId) this.loadManager(this.managerId);
  }

  age(value: unknown): number | null {
    const date = new Date(String(value ?? ''));
    if (Number.isNaN(date.getTime())) return null;
    const today = new Date();
    let years = today.getFullYear() - date.getFullYear();
    if (today.getMonth() < date.getMonth() || (today.getMonth() === date.getMonth() && today.getDate() < date.getDate())) years--;
    return years;
  }

  goBack(): void {
    this.router.navigate(['/manager-list']);
  }

  private loadManager(id: string): void {
    this.loading = true;
    this.error = '';
    this.managerService.getById(id).subscribe({
      next: async (data) => {
        this.selectedManager = {
          firstName: data.firstName,
          lastName: data.lastName,
          dateOfBirth: data.dateOfBirth,
          careerStart: data.careerStart || null,
        };
        this.pictureUrl = pictureToDataUrl(data.picture);
        this.history = await this.historyService.loadManagerHistory(id);
        this.showHistoryTab = hasManagerHistoryData(this.history);
        this.historyRows = this.buildHistoryRows(this.history);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.error = apiErrorMessage(err, 'Manager could not be loaded.');
        this.cdr.detectChanges();
      },
    });
  }

  private buildHistoryRows(form: ManagerHistoryForm): { label: string; value: unknown }[] {
    const rows: { label: string; value: unknown }[] = [];
    const { selection, mandat } = form;
    if (selection.notes?.trim()) rows.push({ label: 'Notes', value: selection.notes });
    if (selection.wins != null) rows.push({ label: 'Wins', value: selection.wins });
    if (selection.loses != null) rows.push({ label: 'Losses', value: selection.loses });
    if (selection.draw != null) rows.push({ label: 'Draws', value: selection.draw });
    if (mandat.startMandatDate) rows.push({ label: 'From', value: mandat.startMandatDate });
    if (mandat.endMandatDate) rows.push({ label: 'To', value: mandat.endMandatDate });
    return rows;
  }
}
