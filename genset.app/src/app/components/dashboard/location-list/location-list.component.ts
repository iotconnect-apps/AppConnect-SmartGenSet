import { Component, OnInit, Input, OnChanges } from '@angular/core'
import { Router } from '@angular/router'
import { NgxSpinnerService } from 'ngx-spinner'
import { MatDialog, MatTableDataSource, MatSort, MatPaginator } from '@angular/material'
import { DeleteDialogComponent } from '../../../components/common/delete-dialog/delete-dialog.component';
import { DeviceService, NotificationService } from 'app/services';
import { Notification } from 'app/services/notification/notification.service';
import { AppConstant, DeleteAlertDataModel } from "../../../app.constants";
import { DashboardService } from 'app/services/dashboard/dashboard.service';

@Component({
	selector: 'app-location-list',
	templateUrl: './location-list.component.html',
	styleUrls: ['./location-list.component.css']
})
export class LocationListComponent implements OnInit {
	changeStatusDeviceName:any;
	changeStatusDeviceStatus:any;
	@Input() parentDeviceId: string;
	order = true;
	isSearch = false;
	pageSizeOptions: number[] = [5, 10, 25, 100];
	reverse = false;
	orderBy = 'name';
	totalRecords = 0;
	searchParameters = {
		pageNo: 0,
		pageSize: 10,
		searchText: '',
		sortBy: 'name asc'
	};
	locationlist:any=[]
	displayedColumns: string[] = ['name', 'city', 'latitude','longitude','action'];
	dataSource = [];
	deleteAlertDataModel: DeleteAlertDataModel;
	constructor(
		private spinner: NgxSpinnerService,
		private router: Router,
		public dialog: MatDialog,
		private deviceService: DeviceService,
		private _notificationService: NotificationService,
		public _appConstant: AppConstant,
		public dashboardService: DashboardService,
	) { }
	
	
	ngOnInit() {
		this.getLocationlist();
	}


	getLocationlist() {
		this.dataSource = [];
		this.spinner.show();
		this.dashboardService.getLocationlist(this.searchParameters).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				//this.lat = response.data.items[0].latitude;
				// this.lng = response.data.items[0].longitude;
				this.totalRecords = response.data.count;
				this.dataSource = response.data.items

			}
			else {
				this.totalRecords = 0;
				this.dataSource = [];
				response.message ? response.message : response.message = "No results found";
				this._notificationService.add(new Notification('error', response.message));
			}
		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
	}

	clickAdd() {
		this.router.navigate(['/location/add']);
	}


	setOrder(sort: any) {
		if (!sort.active || sort.direction === '') {
			return;
		}
		this.searchParameters.sortBy = sort.active + ' ' + sort.direction;
		
		this.getLocationlist();
	}

	deleteModel(id: any) {
		this.deleteAlertDataModel = {
			title: "Delete Location",
			message: this._appConstant.msgConfirm.replace('modulename', "Location"),
			okButtonName: "Yes",
			cancelButtonName: "No",
		};
		const dialogRef = this.dialog.open(DeleteDialogComponent, {
			width: '400px',
			height: 'auto',
			data: this.deleteAlertDataModel,
			disableClose: false
		});
		dialogRef.afterClosed().subscribe(result => {
			if (result) {
				this.deletelocation(id);
			}
		});
	}

	deletelocation(guid) {
		this.spinner.show();
		this.dashboardService.deleteLocation(guid).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				this._notificationService.add(new Notification('success', this._appConstant.msgDeleted.replace("modulename", "Location")));
				this.getLocationlist();

			}
			else {
				this._notificationService.add(new Notification('error', response.message));
			}

		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
	}


	ChangePaginationAsPageChange(pagechangeresponse) {
		this.searchParameters.pageNo = pagechangeresponse.pageIndex;
		this.searchParameters.pageSize = pagechangeresponse.pageSize;
		this.isSearch = true;
		this.getLocationlist();
	}

	searchTextCallback(filterText) {
		this.searchParameters.searchText = filterText;
		this.searchParameters.pageNo = 0;
		this.getLocationlist();
		this.isSearch = true;
	}



	getChildDeviceList() {

		if(!this.parentDeviceId)
			this._notificationService.add(new Notification('error', "Parent device ID is not found"));

		this.spinner.show();
		this.deviceService.getChildDevices(this.parentDeviceId, this.searchParameters).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				this.totalRecords = response.data.count;
				this.dataSource = response.data.items;
			}
			else {
				this._notificationService.add(new Notification('error', response.message));
				this.dataSource = [];
			}
		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
	}

	activeInactiveGenrator(generatorId: string, isActive: boolean, name: string) {
		var status = isActive == false ? this._appConstant.activeStatus : this._appConstant.inactiveStatus;
		var mapObj = {
			statusname: status,
			fieldname: name,
			modulename: "Generator"
		};
		this.deleteAlertDataModel = {
			title: "Status",
			message: this._appConstant.msgStatusConfirm.replace(/statusname|fieldname|modulename/gi, function (matched) {
				return mapObj[matched];
			}),
			okButtonName: "Confirm",
			cancelButtonName: "Cancel",
		};
		const dialogRef = this.dialog.open(DeleteDialogComponent, {
			width: '400px',
			height: 'auto',
			data: this.deleteAlertDataModel,
			disableClose: false
		});
		dialogRef.afterClosed().subscribe(result => {
			if (result) {
				this.changeGeneratorStatus(generatorId, isActive);

			}
		});

	}

	changeGeneratorStatus(generatorId, isActive) {
		this.spinner.show();
		this.deviceService.changegeneratorStatus(generatorId, isActive).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				this._notificationService.add(new Notification('success', this._appConstant.msgStatusChange.replace("modulename", "Generator")));
				this.getGeneratersList();

			}
			else {
				this._notificationService.add(new Notification('error', response.message));
			}

		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
	}

	deleteGenerator(guid) {
		this.spinner.show();
		this.deviceService.deleteGenerator(guid).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				this._notificationService.add(new Notification('success', this._appConstant.msgDeleted.replace("modulename", "Generator")));
				this.getGeneratersList();
			}
			else {
				this._notificationService.add(new Notification('error', response.message));
			}
		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
	}

	getGeneratersList() {
		this.spinner.show();
		//this.deviceService.getgeneraters().subscribe(response => {
		this.deviceService.getgeneratorsSearch(this.searchParameters).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				this.totalRecords = response.data.count;
				this.dataSource = response.data.items;
			}
			else {
				this._notificationService.add(new Notification('error', response.message));
				this.dataSource = [];
			}
		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error ));
		});
	}
	
}
