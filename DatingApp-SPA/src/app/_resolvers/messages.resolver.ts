import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { catchError } from 'rxjs/operators';
import { Message } from '../_models/Message';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MessagesResolver implements Resolve<Message[]> {


    pageNumber = 1;
    pageSize = 5;
    messageContainer = 'Unread';
    constructor(private userService: UserService, private authService: AuthService,
                private router: Router, private alertify: AlertifyService ) {}
    resolve(route: ActivatedRouteSnapshot): Observable<Message[]> {

        return this.userService.getMessages(this.authService.decodedToken.nameid, this.pageNumber, this.pageSize, this.messageContainer)
        .pipe(
            catchError( error => {
                console.log(error);
                this.alertify.error(error);
                this.router.navigate(['']);
                return of(null);
            })
        );
    }
}
