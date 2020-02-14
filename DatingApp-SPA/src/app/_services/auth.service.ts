import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { BehaviorSubject } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = 'http://localhost:5000/api/auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;

  photoUrl = new BehaviorSubject<string>('../../assets/user.png');
  currentPhotoUrl = this.photoUrl.asObservable();

constructor(private http: HttpClient) { }

changeMemberPhoto(photoUrl: string) {
  this.photoUrl.next(photoUrl);
}

updateMainPhoto(photoUrl: string) {
  this.changeMemberPhoto(photoUrl);
  // Now update the local storage cache, so that when client is refreshed the changes are persistent
  this.currentUser.photoUrl = photoUrl;
  localStorage.setItem('user', JSON.stringify(this.currentUser));
}

login(user: User) {
  return this.http.post(this.baseUrl + 'login', user)
  .pipe(
  map((response: any) => {
    const repUser = response;
    if (repUser) {
      localStorage.setItem('token', repUser.token);
      localStorage.setItem('user', JSON.stringify(repUser.user));
      this.decodedToken = this.jwtHelper.decodeToken(repUser.token);
      this.currentUser = repUser.user;
      this.changeMemberPhoto(this.currentUser.photoUrl);
    }
  }));
}

register(user: User) {

  return this.http.post(this.baseUrl + 'register', user);

}

loggedIn() {

  if (!this.decodedToken) {
    this.decodedToken = this.jwtHelper.decodeToken(localStorage.getItem('token'));
  }
  if (!this.currentUser) {
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (user) {
      this.currentUser = user;
      this.changeMemberPhoto(this.currentUser.photoUrl);
    }
  }
  return !this.jwtHelper.isTokenExpired(localStorage.getItem('token'));
}

logout() {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  this.currentUser = null;
  this.decodedToken = null;
}

}
