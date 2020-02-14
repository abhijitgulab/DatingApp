import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { tokenName } from '@angular/compiler';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {};
  photoUrl: string;

  constructor(public authService: AuthService, private alertify: AlertifyService, private router: Router) { }

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(photoUrl => {
      this.photoUrl = photoUrl;
    });
  }

  login() {
    this.authService.login(this.model).subscribe( next => {
      console.log('Logged in successfully');
      this.alertify.success('Logged in successfully');
    }, error => {
      console.log(error);
      this.alertify.error(error);
    }, () => {
      this.router.navigate(['/members']);
    }
    );
  }

  loggedIn() {
    return this.authService.loggedIn();
  }

  logout() {
    this.authService.logout();
    console.log('Logged out');
    this.alertify.message('Logged out');
    this.router.navigate(['']);
  }

}
