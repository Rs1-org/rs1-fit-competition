export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expires: string;
  user: UserDto;
  roles: string[];
}

export interface AuthApiResponse {
  accessToken: string;
  refreshToken: string;
  expires: string;
  user: UserDto;
  roles: string[];
}