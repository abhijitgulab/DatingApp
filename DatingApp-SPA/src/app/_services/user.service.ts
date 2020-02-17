import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { environment } from 'src/environments/environment';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserService {

baseUrl = environment.apiUrl + 'users';
constructor(private httpClient: HttpClient) { }

getUsers(page?, itemsPerPage?, userParams?): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();
    let params = new HttpParams();
    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }
    if(userParams != null) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }
    return this.httpClient.get<User[]>(this.baseUrl, {observe: 'response', params})
    .pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') != null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
}

getUser(id): Observable<User> {
    return this.httpClient.get<User>(this.baseUrl + '/' + id);
}

updateUser(id: number, user: User) {
  return this.httpClient.put(this.baseUrl + '/' + id, user);
}

setMainPhoto(userId: number, id: number) {
  return this.httpClient.post(this.baseUrl + '/' + userId + '/photos/' + id + '/setMain', {});
}

deletePhoto(userId: number, id: number) {
  return this.httpClient.delete(this.baseUrl + '/' + userId + '/photos/' + id );
}

}
