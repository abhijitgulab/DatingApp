import { Component, OnInit, HostListener, ViewChild } from '@angular/core';
import { AuthService } from './_services/auth.service';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    if (this.authService.loggedIn()) {
        console.log('User - ' + this.authService.decodedToken.unique_name + ' already logged in');
    }
  }
}
