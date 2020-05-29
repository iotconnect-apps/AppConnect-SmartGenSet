import { Component, OnInit } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AppConstant, DeleteAlertDataModel } from "../../../app.constants";
import { MatDialog, MatTableDataSource, MatSort, MatPaginator } from '@angular/material'
import { DeleteDialogComponent } from '../../../components/common/delete-dialog/delete-dialog.component';
import * as moment from 'moment-timezone'
import { LocationService, DeviceService, Notification, NotificationService, AlertsService } from '../../../services';
@Component({
  selector: 'app-location-detail',
  templateUrl: './location-detail.component.html',
  styleUrls: ['./location-detail.component.css']
})
export class LocationDetailComponent implements OnInit {
  locationGuid: any;
  totalOnConnectedGenerators: any;
  totalOffGenerators: any;
  totalDisconnectedGenerators: any;
  totalEneryGenerated: any;
  totalFuelUsed: any;
  locationname: any;
  generators: [];
  deleteAlertDataModel: DeleteAlertDataModel;

  ChartHead = ['Date/Time'];
  chartData = [];
  datadevice: any = [];
  columnArray: any = [];
  headFormate: any = {
    columns: this.columnArray,
    type: 'NumberFormat'
  };
  bgColor = '#fff';
  chartHeight = 300;
  chartWidth = '100%';
  chart = {
    'fuelUsage': {
      chartType: 'ColumnChart',
      dataTable: [],
      options: {
        title: "",
        bar: { groupWidth: "25%" },
        colors: ['#ed734c'],
        height: this.chartHeight,
        width: this.chartWidth,
        interpolateNulls: true,
        backgroundColor: this.bgColor,
        hAxis: {
          title: 'Date/Time',
          titleTextStyle: {
            bold: true
          },
        },
        vAxis: {
          title: 'Values',
          titleTextStyle: {
            bold: true
          },
          viewWindow: {
            min: 0
          },
        }
      },
      formatters: this.headFormate
    },
    'energyConsumption': {
      chartType: 'ColumnChart',
      dataTable: [],
      options: {
        title: "",
        bar: { groupWidth: "25%" },
        colors: ['#5496d0'],
        height: this.chartHeight,
        width: this.chartWidth,
        interpolateNulls: true,
        backgroundColor: this.bgColor,
        hAxis: {
          title: 'Date/Time',
          titleTextStyle: {
            bold: true
          },
        },
        vAxis: {
          title: 'Values',
          titleTextStyle: {
            bold: true
          },
          viewWindow: {
            min: 0
          },
        }
      },
      formatters: this.headFormate
    },
    'generaytorBatteryStatus': {
      chartType: 'ColumnChart',
      dataTable: [],
      options: {
        title: "",
        bar: { groupWidth: "25%" },
        colors: ['#5496d0'],
        height: this.chartHeight,
        width: this.chartWidth,
        interpolateNulls: true,
        backgroundColor: this.bgColor,
        hAxis: {
          title: 'Date/Time',
          titleTextStyle: {
            bold: true
          },
        },
        vAxis: {
          title: 'Values',
          titleTextStyle: {
            bold: true
          },
          viewWindow: {
            min: 0
          }
        }
      },
      formatters: this.headFormate
    },
    'generatorUsage': {
      chartType: 'PieChart',
      dataTable: [],
      options: {
        title: "",
        bar: { groupWidth: "25%" },
        colors: ['#ed734c'],
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
    private spinner: NgxSpinnerService,
    private locationService: LocationService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    public _appConstant: AppConstant,
    public dialog: MatDialog,
    private deviceService: DeviceService,
    private location: Location,
    private _notificationService: NotificationService,
    public _service: AlertsService
  ) { }


  ngOnInit() {
    this.activatedRoute.params.subscribe(params => {
      // set data for parent device
      this.locationGuid = params.locationGuid;
      this.getlocationstatics(this.locationGuid)
      this.getLocationdetail(this.locationGuid);
      this.getEnergyUsageChartData();
      this.getGeneraytorBatteryStatusChartData();
      this.getFuelUsageChartData();
      this.getGeneraterUsagePieChartData();
      this.getAlertList();
    });
  }
  getLocalDate(lDate) {
    var utcDate = moment.utc(lDate, 'YYYY-MM-DDTHH:mm:ss.SSS');
    // Get the local version of that date
    var localDate = moment(utcDate).local();
    let res = moment(localDate).format('MMM DD, YYYY hh:mm:ss A');
    return res;

  }
  getAlertList() {
    this.spinner.show();
    let parameters = {
      pageNumber: 0,
      pageSize: 10,
      orderBy: 'eventDate desc',
      deviceGuid: '',
      entityGuid: this.locationGuid,
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

  getFuelUsageChartData() {
    let obj = { companyGuid: this.currentUser.userDetail.companyId, entityGuid: this.locationGuid };
    let data = [
      ['Months', 'Fuel']
    ]
    this.deviceService.getFuelUsageChartData(obj).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true) {
        if (response.data.length) {
          response.data.forEach(element => {
            data.push([element.month, parseFloat(element.value)]);
          });
          this.createHistoryChart('fuelUsage', data, 'Months', 'gal', '#ed734c');
        } else {
          this.chart.fuelUsage.dataTable = [];
        }
      }
      else {
        this.chart.fuelUsage.dataTable = [];
        this._notificationService.add(new Notification('error', response.message));
      }
    }, error => {
      this.chart.fuelUsage.dataTable = [];
      this.spinner.hide();
      this._notificationService.add(new Notification('error', error));
    });


  }

  getEnergyUsageChartData() {
    this.spinner.show();
    let obj = { companyGuid: this.currentUser.userDetail.companyId, entityGuid: this.locationGuid };
    let data = [
      ['Months', 'Energy']
    ]
    this.deviceService.getEnergyUsageChartData(obj).subscribe(response => {

      if (response.isSuccess === true) {
        if (response.data.length) {
          response.data.forEach(element => {
            data.push([element.month, parseFloat(element.value)]);
          });
          this.createHistoryChart('energyConsumption', data, 'Months', 'KWH', '#5496d0');
        } else {
          this.chart.energyConsumption.dataTable = [];
        }
        this.spinner.hide();
      }
      else {
        this.spinner.hide();
        this.chart.energyConsumption.dataTable = [];
        this._notificationService.add(new Notification('error', response.message));
      }
    }, error => {
      this.chart.energyConsumption.dataTable = [];
      this.spinner.hide();
      this._notificationService.add(new Notification('error', error));
    });


  }

  getGeneraytorBatteryStatusChartData() {
    let obj = { companyGuid: this.currentUser.userDetail.companyId, entityGuid: this.locationGuid };
    let data = [
      ['generator', 'Bettery Status']
    ]
    this.deviceService.getGeneraytorBatteryStatusChartData(obj).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true) {
        if (response.data.length) {
          response.data.forEach(element => {
            data.push([element.month, parseFloat(element['value'])]);
          });
          this.createHistoryChart('generaytorBatteryStatus', data, 'Generator', '% Percentage', '#5496d0');
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

  getGeneraterUsagePieChartData() {
    let obj = { companyGuid: this.currentUser.userDetail.companyId, entityGuid: this.locationGuid };
    let data = [
      ['Generator', 'kVA']
    ]
    this.deviceService.getGeneraterUsagePieChartData(obj).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true) {
        if (response.data.length) {
          response.data.forEach(element => {
            data.push([element['uniqueId'], parseFloat(element['value'])]);
          });
          this.createHistoryChart('generatorUsage', data, '', '', '#ed734c');
        } else {
          this.chart.generatorUsage.dataTable = [];
        }
      }
      else {
        this.chart.generatorUsage.dataTable = [];
        this._notificationService.add(new Notification('error', response.message));
      }
    }, error => {
      this.chart.generatorUsage.dataTable = [];
      this.spinner.hide();
      this._notificationService.add(new Notification('error', error));
    });

  }
  createHistoryChart(key, data, hAxisTitle, vAxisTitle, color) {
    let chartType = 'ColumnChart';
    var legends = { position: 'none' };
    if (key === 'generatorUsage') {
      chartType = 'PieChart';
      legends = { position: 'right' };
      //color = '';
      
    }
    let height = this.chartHeight;
    this.chart[key] = {
      chartType: chartType,
      dataTable: data,
      options: {
        bar: { groupWidth: "25%" },
        colors: [color],
        legend: legends,
        height: height,
        width: this.chartWidth,
        interpolateNulls: true,
        backgroundColor: this.bgColor,
        hAxis: {
          title: hAxisTitle,
          titleTextStyle: {
            bold: true
          },
        },
        vAxis: {
          title: vAxisTitle,
          titleTextStyle: {
            bold: true
          },
          viewWindow: {
            min: 0
          },
        }
      },
      formatters: this.headFormate
    };
  }
  clickBack() {
    this.location.back();
    // this.router.navigate(['dashboard']);
  }
  getLocationdetail(locationId) {
    this.spinner.show();
    this.locationService.getLocationdetail(locationId).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true) {
        this.locationname = response.data.name
        this.generators = response.data.generators
      }
      else {
        //this._notificationService.add(new Notification('error', response.message));

      }
    }, error => {
      this.spinner.hide();
      //this._notificationService.add(new Notification('error', error));
    });
  }

  getlocationstatics(locationId) {
    this.spinner.show();
    this.locationService.getlocationstatics(locationId).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true) {
        this.totalOnConnectedGenerators = response.data.totalOnGenerators
        this.totalOffGenerators = response.data.totalOffGenerators
        this.totalDisconnectedGenerators = response.data.totalDisconnectedGenerators
        this.totalEneryGenerated = response.data.totalEnergyGenerated
        this.totalFuelUsed = response.data.totalFuelUsed
      }
      else {
        //this._notificationService.add(new Notification('error', response.message));

      }
    }, error => {
      this.spinner.hide();
      //this._notificationService.add(new Notification('error', error));
    });
  }

  deleteModel(GeneratorModel: any) {
    this.deleteAlertDataModel = {
      title: "Delete Generator",
      message: this._appConstant.msgConfirm.replace('modulename', "generator"),
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
        this.deleteGenerator(GeneratorModel.guid);
      }
    });
  }
  deleteGenerator(guid) {
    this.spinner.show();
    this.deviceService.deleteGenerator(guid).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true) {
        this._notificationService.add(new Notification('success', this._appConstant.msgDeleted.replace("modulename", "Generator")));
        this.router.navigate(['location-detail', , this.locationGuid]);
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
