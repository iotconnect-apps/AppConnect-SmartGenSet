import * as moment from 'moment-timezone'
import { Component, OnInit } from '@angular/core'
import { NgxSpinnerService } from 'ngx-spinner'
import { DashboardService } from 'app/services/dashboard/dashboard.service';
import { Notification, NotificationService } from 'app/services/notification/notification.service';
import { Router } from '@angular/router'
import { AppConstant, DeleteAlertDataModel } from "../../app.constants";
import { MatDialog, MatPaginator, MatSort, MatTableDataSource } from '@angular/material'
import { DeleteDialogComponent } from '../../components/common/delete-dialog/delete-dialog.component';
import { locationobj } from './dashboard-model';
import { DeviceService, AlertsService } from 'app/services';

@Component({
	selector: 'app-dashboard',
	templateUrl: './dashboard.component.html',
	styleUrls: ['./dashboard.component.css'],
})

export class DashboardComponent implements OnInit {
	locationobj = new locationobj();
	lat = 32.897480;
	lng = -97.040443;
	//lat = '';
	//lng = '';
	locationlist: any = [];
	isShowLeftMenu = true;
	mapview = true;
	totalAlerts: any;
	totalDisconnectedGenerators: any;
	totalEneryGenerated: any;
	totalFuelUsed: any;
	totalGenerators: any;
	totalLocations: any;
	totalOffGenerators: any;
	totalOnGenerators: any;
	deleteAlertDataModel: DeleteAlertDataModel;
	searchParameters = {
		pageNo: 0,
		pageSize: 10,
		searchText: '',
		sortBy: 'uniqueId asc'
	};
	ChartHead = ['Date/Time'];
	chartData = [];
	datadevice: any = [];
	columnArray: any = [];
	headFormate: any = {
		columns: this.columnArray,
		type: 'NumberFormat'
	};
	bgColor = '#fff';
	chartHeight = 320;
	chartWidth = '100%';
	chart = {
		'generaytorBatteryStatus': {
			chartType: 'ColumnChart',
			dataTable: [],
			options: {
				height: this.chartHeight,
				width: this.chartWidth,
				interpolateNulls: true,
				backgroundColor: this.bgColor,
				hAxis: {
					title: 'Date/Time',
					gridlines: {
						count: 5
					},
				},
				vAxis: {
					title: 'Values',
					gridlines: {
						count: 1
					},
				}
			},
			formatters: this.headFormate
		}
	};
	currentUser = JSON.parse(localStorage.getItem('currentUser'));
	alerts = [];
	constructor(
		private router: Router,
		private spinner: NgxSpinnerService,
		private dashboardService: DashboardService,
		private _notificationService: NotificationService,
		public _appConstant: AppConstant,
		public dialog: MatDialog,
		private deviceService: DeviceService,
		public _service: AlertsService

	) { }

	ngOnInit() {
		this.getDashbourdCount();
		this.getLocationlist();
		this.getGeneraytorBatteryStatusChartData();
		this.getAlertList();
	}

	getAlertList() {
		this.spinner.show();
		let parameters = {
			pageNumber: 0,
			pageSize: 10,
			orderBy: 'eventDate desc',
			deviceGuid: '',
			entityGuid: '',
		};
		this._service.getAlerts(parameters).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true && response.data.items) {
				this.alerts = response.data.items;
			}
			else {
				this.alerts = [];
				this._notificationService.add(new Notification('error', response.message));
			}
		}, error => {
			this.alerts = [];
			this._notificationService.add(new Notification('error', error));
		});
	}

	getGeneraytorBatteryStatusChartData() {
		let obj = { companyGuid: this.currentUser.userDetail.companyId };
		let data = []
		this.deviceService.getGeneraytorBatteryStatusChartData(obj).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				if (response.data.length) {
					data.push(['generator', 'Bettery Status']);

					response.data.forEach(element => {
						data.push([element.name, parseInt(element['value'])]);
					});
					this.createHistoryChart('generaytorBatteryStatus', data, 'Generator', '% Percentage');
				} else {
					this.chart.generaytorBatteryStatus.dataTable = [];
				}
			}
			else {
				this.chart.generaytorBatteryStatus.dataTable = [];
				this._notificationService.add(new Notification('error', response.message));
			}
		}, error => {
			this.chart.generaytorBatteryStatus.dataTable = [];
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});


	}
	createHistoryChart(key, data, hAxisTitle, vAxisTitle) {
		let height = this.chartHeight;
		this.chart[key] = {
			chartType: 'ColumnChart',
			dataTable: data,
			options: {
				height: height,
				width: this.chartWidth,
				interpolateNulls: true,
				backgroundColor: this.bgColor,
				hAxis: {
					title: hAxisTitle,
					gridlines: {
						count: 5
					},
				},
				vAxis: {
					title: vAxisTitle,
					gridlines: {
						count: 1
					},
				}
			},
			formatters: this.headFormate
		};
	}

	clickAdd() {
		this.router.navigate(['location/add']);
	}
	clickDetail(id) {
		this.router.navigate(['location-detail', id]);
	}
	convertToFloat(value) {
		return parseFloat(value)
	}

	/**
	 * Get count of variables for Dashboard
	 * */
	getDashbourdCount() {
		this.spinner.show();
		this.dashboardService.getDashboardoverview().subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				this.totalAlerts = response.data.totalAlerts
				this.totalDisconnectedGenerators = response.data.totalDisconnectedGenerators
				this.totalEneryGenerated = response.data.totalEneryGenerated
				this.totalFuelUsed = response.data.totalFuelUsed
				this.totalGenerators = response.data.totalGenerators
				this.totalLocations = response.data.totalLocations
				this.totalOffGenerators = response.data.totalOffGenerators
				this.totalOnGenerators = response.data.totalOnGenerators
			}
			else {
				this._notificationService.add(new Notification('error', response.message));
			}
		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
	}
	search(filterText) {
		this.searchParameters.searchText = filterText;
		this.searchParameters.pageNo = 0;
		this.getLocationlist();
	}
	getLocationlist() {
		this.locationlist = [];
		this.spinner.show();
		this.dashboardService.getLocationlist(this.searchParameters).subscribe(response => {
			this.spinner.hide();
			if (response.isSuccess === true) {
				//this.lat = response.data.items[0].latitude;
				// this.lng = response.data.items[0].longitude;
				this.locationlist = response.data.items

			}
			else {
				// response.message ? response.message : response.message = "No results found";
				this._notificationService.add(new Notification('error', response.message));
			}
		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
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


}
