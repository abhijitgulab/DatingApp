import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {

  @Input() photos: Photo[];
  @Output() getMemberPhotoChange = new EventEmitter<string>();

  baseUrl = environment.apiUrl;
  uploader: FileUploader;
  hasBaseDropZoneOver: false;
  currentMainPhoto: Photo;
  constructor(private authService: AuthService, private userService: UserService, private alertify: AlertifyService) { }

  initializeUploader() {
    this.uploader = new FileUploader ({
      url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });
    this.uploader.onAfterAddingFile = (file) => { file.withCredentials = false; };
    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        this.photos.push(res);
        if (res.isMain) {
          this.authService.updateMainPhoto(res.url);
        }
      }
    };
  }

  ngOnInit() {
    this.initializeUploader();
  }

   fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  setMainPhoto(photo: Photo) {
    this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id).subscribe( next => {
      this.currentMainPhoto = this.photos.filter(p => p.isMain === true)[0];
      this.currentMainPhoto.isMain = false;
      photo.isMain = true;
      this.getMemberPhotoChange.emit(photo.url);
      this.authService.updateMainPhoto(photo.url);
    }, error => {
      this.alertify.error('Photo not successfully set as main');
    });

  }

  deletePhoto(id: number) {
    this.alertify.confirm('Are you sure you want to delete?', () => {
      this.userService.deletePhoto(this.authService.decodedToken.nameid, id).subscribe(next => {
        this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
      }, error => {
        this.alertify.error('Photo cannot be deleted');
      });
    });
  }

}
