import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {

baseUrl = environment.apiUrl + 'users';
constructor(private httpClient: HttpClient) { }

getUsers(): Observable<User[]> {
    return this.httpClient.get<User[]>(this.baseUrl);
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
