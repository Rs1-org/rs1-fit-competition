import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AdminUserService } from './admin-user.service';
import { environment } from '../../../environments/environment';

describe('Admin user API contracts', () => {
  it('preserves paging/false filters and sends only the intended mutation fields', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(AdminUserService);
    const requests = TestBed.inject(HttpTestingController);
    const url = `${environment.apiUrl}/api/admin/users`;
    service.getAll(2, 10, { isActive: false, role: 'User' }).subscribe();
    const list = requests.expectOne(request => request.url === url);
    expect(list.request.params.get('page')).toBe('2');
    expect(list.request.params.get('isActive')).toBe('false');
    expect(list.request.params.get('role')).toBe('User');
    list.flush({ items: [], totalCount: 0 });
    service.changeRole('a'.repeat(32), 'Moderator').subscribe();
    const role = requests.expectOne(`${url}/${'a'.repeat(32)}/role`);
    expect(role.request.body).toEqual({ role: 'Moderator' }); role.flush(true);
    service.setActive('a'.repeat(32), false).subscribe();
    const active = requests.expectOne(`${url}/${'a'.repeat(32)}/active`);
    expect(active.request.body).toEqual({ isActive: false }); active.flush(true);
    requests.verify();
  });
});
