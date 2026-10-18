import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { Subject } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { RegisterComponent } from './register.component';

describe('Registration validation', () => {
  let component: RegisterComponent;
  const register = vi.fn();
  const valid = {
    email: 'dev@example.invalid', password: 'Valid!aA123', confirmPassword: 'Valid!aA123',
    firstName: 'Dev', lastName: 'Test', phoneNumber: '061111111',
  };

  beforeEach(() => {
    register.mockReset();
    TestBed.configureTestingModule({ providers: [{ provide: AuthService, useValue: { register } }] });
    component = TestBed.runInInjectionContext(() => new RegisterComponent());
  });

  it('rejects weak passwords and mismatched confirmations before sending', () => {
    component.form.setValue({ ...valid, password: 'weak', confirmPassword: 'other' });
    component.submit();
    expect(component.form.invalid).toBe(true);
    expect(register).not.toHaveBeenCalled();
    expect(component.errors.join(' ')).toContain('passwords must match');
  });

  it('includes confirmation in the payload, prevents duplicate submissions and retains values on failure', () => {
    const response = new Subject<unknown>();
    register.mockReturnValue(response);
    component.form.setValue(valid);
    component.submit();
    component.submit();
    expect(register).toHaveBeenCalledExactlyOnceWith(valid);
    response.error(new HttpErrorResponse({ status: 400, error: {
      message: 'Validation failed', errors: [{ propertyName: 'email', errorMessage: 'This email is already registered.' }],
    } }));
    expect(component.isSubmitting).toBe(false);
    expect(component.form.getRawValue()).toEqual(valid);
    expect(component.errors).toEqual(['This email is already registered.']);
  });
});
