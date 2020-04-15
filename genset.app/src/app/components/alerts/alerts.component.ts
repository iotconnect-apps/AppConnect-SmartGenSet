import { Component, OnInit } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner'
import { NotificationService, Notification, AlertsService } from '../../services';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-alerts',
  templateUrl: './alerts.component.html',
  styleUrls: ['./alerts.component.css']
})
export class AlertsComponent implements OnInit {
  alerts = [];
  displayedColumns: string[] = ['message', 'entityName', 'deviceName', 'eventDate', 'severity'];
  order = true;
  isSearch = false;
  reverse = false;
  orderBy = 'name';
  pageSizeOptions: number[] = [5, 10, 25, 100];
  searchParameters = {
    pageNumber: 0,
    pageSize: 10,
    orderBy: 'eventDate desc',
    deviceGuid: '',
    entityGuid: '',
  };
  totalRecords = 0;
  constructor(
    private spinner: NgxSpinnerService,
    public _service: AlertsService,
    private _notificationService: NotificationService,
    private route: ActivatedRoute) { }

  ngOnInit() {

    this.route.queryParams.subscribe(params => {
      if (params['generatorGuid']) {
        this.searchParameters.deviceGuid = params['generatorGuid'];
      }
      if (params['locationGuid']) {
        this.searchParameters.entityGuid = params['locationGuid'];
      }
    });
    this.getAlertList();
  }

  getAlertList() {
    this.spinner.show();
    this._service.getAlerts(this.searchParameters).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true && response.data.items) {
        this.alerts = response.data.items;
        this.totalRecords = response.data.count;
      }
      else {
        this.alerts = [];
        this._notificationService.add(new Notification('error', response.message));
        this.totalRecords = 0;
      }
    }, error => {
      this.alerts = [];
      this.totalRecords = 0;
      this._notificationService.add(new Notification('error', error));
    });
  }


  setOrder(sort: any) {
    if (!sort.active || sort.direction === '') {
      return;
    }
    this.searchParameters.orderBy = sort.active + ' ' + sort.direction;
    this.getAlertList();
  }


  onPageSizeChangeCallback(pageSize) {
    this.searchParameters.pageSize = pageSize;
    this.searchParameters.pageNumber = 1;
    this.isSearch = true;
    this.getAlertList();
  }

  ChangePaginationAsPageChange(pagechangeresponse) {
    this.searchParameters.pageNumber = pagechangeresponse.pageIndex;
    this.searchParameters.pageSize = pagechangeresponse.pageSize;
    this.isSearch = true;
    this.getAlertList();
  }

  searchTextCallback(filterText) {
    this.searchParameters.pageNumber = 0;
    this.getAlertList();
    this.isSearch = true;
  }

}
