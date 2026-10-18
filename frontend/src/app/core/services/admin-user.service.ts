import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AdminUser, UserFilter } from '../models/admin-user';
import { PagedResult } from '../models/paged-result';
import { toPagedParams } from '../helpers/paging.helper';

@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/api/admin/users`;

  getAll(page = 1, pageSize = 10, filter: UserFilter = {}) {
    return this.http.get<PagedResult<AdminUser>>(this.url, { params: toPagedParams(page, pageSize, filter) });
  }
  changeRole(publicId: string, role: string) {
    return this.http.put<boolean>(`${this.url}/${encodeURIComponent(publicId)}/role`, { role });
  }
  setActive(publicId: string, isActive: boolean) {
    return this.http.put<boolean>(`${this.url}/${encodeURIComponent(publicId)}/active`, { isActive });
  }
}
