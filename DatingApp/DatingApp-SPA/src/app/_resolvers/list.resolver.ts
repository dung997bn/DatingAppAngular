import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ListResolver implements Resolve<User[]>{

    pageNumber = 1;
    pageSize = 5;
    likesParam = 'Likers';

    constructor(private userSevice: UserService,
        private router: Router,
        private alertify: AlertifyService) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<User[]> {
        return this.userSevice.getUsers(this.pageNumber, this.pageSize, null, this.likesParam).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving data.');
                this.router.navigate(['/home']);
                return of(null);
            })
        )
    }

}