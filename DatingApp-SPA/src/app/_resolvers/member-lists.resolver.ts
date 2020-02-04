import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { catchError } from 'rxjs/operators';

@Injectable()
export class MemberListsResolver implements Resolve<User[]> {

    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService ) {}
    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {

        return this.userService.getUsers().pipe(
            catchError( error => {
                console.log(error);
                this.alertify.error(error);
                this.router.navigate(['']);
                return of(null);
            })
        );
    }
}
