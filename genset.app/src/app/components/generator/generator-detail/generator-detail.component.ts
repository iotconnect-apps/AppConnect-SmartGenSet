import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DeviceService, NotificationService, AlertsService } from 'app/services';
import { NgxSpinnerService } from 'ngx-spinner';
import { Notification } from 'app/services/notification/notification.service';
import * as FileSaver from 'file-saver';
import { FormControl, FormGroup, Validators } from '@angular/forms'
import { DeleteAlertDataModel, AppConstant, MesageAlertDataModel } from 'app/app.constants';
import { MatDialog } from '@angular/material';
import { DeleteDialogComponent } from '../../../components/common/delete-dialog/delete-dialog.component';
import 'chartjs-plugin-streaming';
import { StompRService } from '@stomp/ng2-stompjs'
import { Message } from '@stomp/stompjs'
import { Subscription } from 'rxjs'
import { Observable, forkJoin } from 'rxjs';
import { Location } from '@angular/common';
import * as moment from 'moment-timezone'
import * as _ from 'lodash'
import { MessageDialogComponent } from '../..';
import { arrayToHash } from '@fullcalendar/core/util/object';

@Component({
  selector: 'app-generator-detail',
  templateUrl: './generator-detail.component.html',
  styleUrls: ['./generator-detail.component.css'],
  providers: [StompRService]
})
export class GeneratorDetailComponent implements OnInit {
  validstatus = false;
  MesageAlertDataModel: MesageAlertDataModel;
  subscribed;
  stompConfiguration = {
    url: '',
    headers: {
      login: '',
      passcode: '',
      host: ''
    },
    heartbeat_in: 0,
    heartbeat_out: 2000,
    reconnect_delay: 5000,
    debug: true
  }
  chartColors: any = {
    red: 'rgb(255, 99, 132)',
    orange: 'rgb(255, 159, 64)',
    yellow: 'rgb(255, 205, 86)',
    green: 'rgb(75, 192, 192)',
    blue: 'rgb(54, 162, 235)',
    purple: 'rgb(153, 102, 255)',
    grey: 'rgb(201, 203, 207)',
    cerise: 'rgb(255,0,255)',
    popati: 'rgb(0,255,0)',
    dark: 'rgb(5, 86, 98)',
    solid: 'rgb(98, 86, 98)'
  };
  datasets: any[] = [
    {
      label: 'Dataset 1 (linear interpolation)',
      backgroundColor: 'rgb(153, 102, 255)',
      borderColor: 'rgb(153, 102, 255)',
      fill: false,
      lineTension: 0,
      borderDash: [8, 4],
      data: []
    }
  ];

  options: any = {
    type: 'line',
    scales: {

      xAxes: [{
        type: 'realtime',
        time: {
          stepSize: 10
        },
        realtime: {
          duration: 90000,
          refresh: 1000,
          delay: 2000,
          //onRefresh: '',

          // delay: 2000

        }

      }],
      yAxes: [{
        scaleLabel: {
          display: true,
          labelString: 'value'
        }
      }]

    },
    tooltips: {
      mode: 'nearest',
      intersect: false
    },
    hover: {
      mode: 'nearest',
      intersect: false
    }

  };
  deviceIsConnected = false;
  checkSubmitStatus = false;
  selectedFiles: any = [];
  imageForm: FormGroup;
  mediaUrl: any;
  batteryStatus: any;
  current: any;
  engine: any;
  engineOilLevel: any;
  fuelLevel: any;
  voltage: any;
  dataobj: any = {}
  mediaFiles: any = [];
  public respondShow: boolean = false;
  generatordetailGuid: any;
  cpId = '';
  subscription: Subscription;
  messages: Observable<Message>;
  Respond() {
    this.respondShow = !this.respondShow;
    //this.refresh();
  }
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

  };
  currentUser = JSON.parse(localStorage.getItem('currentUser'));
  alerts = [];
  sensorData = {
    engine_rpm: 0,
    currentout: 0,
    batt_voltage: 0,
    fuel_level: 0,
    fuel_used: 0,
    batt_level: 0,
  }
  isConnected = false;
  constructor(
    private activatedRoute: ActivatedRoute,
    private deviceService: DeviceService,
    private spinner: NgxSpinnerService,
    private _notificationService: NotificationService,
    public _appConstant: AppConstant,
    public dialog: MatDialog,
    private stompService: StompRService,
    public location: Location,
    public _service: AlertsService
  ) {
    this.dataobj = { name: '', uniqueId: '' }
    this.activatedRoute.params.subscribe(params => {
      // set data for parent device
      if (params.generatordetailGuid != null) {
        this.generatordetailGuid = params.generatordetailGuid;
        //this.getgenraterDetail(params.generatordetailGuid);
        this.getgenraterTelemetry(params.generatordetailGuid);
        this.getgenraterMedia(params.generatordetailGuid);
      }
    });
  }

  ngOnInit() {
    this.createFormGroup();
    // this.getStompConfig();
    this.getEnergyUsageChartData();
    //this.getSoilnutritionChartData();
    this.getFuelUsageChartData();
    this.getAlertList();
  }

  getDeviceStatus(uniqueId) {
		this.spinner.show();
		this.deviceService.getDeviceStatus(uniqueId).subscribe(response => {
			if (response.isSuccess === true) {
				this.isConnected = response.data.isConnected;
				this.spinner.hide();
			} else {
				this._notificationService.add(new Notification('error', response.message));
			}
		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
	}


  getgenraterDetail(genraterGuid) {

    this.spinner.show();
    this.deviceService.getgenraterStatistics(genraterGuid).subscribe(response => {
      if (response.isSuccess === true) {

        this.spinner.hide();
        this.getDeviceStatus(response.data.uniqueId)
        this.batteryStatus = response.data.batteryStatus
        this.current = response.data.current
        this.engine = response.data.engine
        this.engineOilLevel = response.data.engineOilLevel
        this.fuelLevel = response.data.fuelLevel
        this.voltage = response.data.voltage
      } else {
        this._notificationService.add(new Notification('error', response.message));
      }
    }, error => {
      this.spinner.hide();
      this._notificationService.add(new Notification('error', error));
    });
  }

  getgenraterTelemetry(genraterGuid) {

    this.spinner.show();
    this.deviceService.getgenraterTelemetry(genraterGuid).subscribe(response => {
      if (response.isSuccess === true) {
        response.data.forEach(element => {
          if (element.attributeName === "engine_rpm") {
            this.sensorData.engine_rpm = element.attributeValue;
          } else if (element.attributeName === "currentout") {
            this.sensorData.currentout = element.attributeValue;
          } else if (element.attributeName === "batt_voltage") {
            this.sensorData.batt_voltage = element.attributeValue;
          } else if (element.attributeName === "fuel_level") {
            this.sensorData.fuel_level = element.attributeValue;
          } else if (element.attributeName === "fuel_used") {
            this.sensorData.fuel_used = element.attributeValue;
          } else if (element.attributeName === "batt_level") {
            this.sensorData.batt_level = element.attributeValue;
          }

        });
        this.spinner.hide();

        // this.batteryStatus = response.data.batteryStatus
        // this.current = response.data.current
        // this.engine = response.data.engine
        // this.engineOilLevel = response.data.engineOilLevel
        // this.fuelLevel = response.data.fuelLevel
        //this.voltage = response.data.voltage
      } else {
        this._notificationService.add(new Notification('error', response.message));
      }
    }, error => {
      this.spinner.hide();
      this._notificationService.add(new Notification('error', error));
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
      deviceGuid: this.generatordetailGuid,
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


  getFuelUsageChartData() {
    let obj = { companyGuid: this.currentUser.userDetail.companyId, deviceGuid: this.generatordetailGuid };
    let data = []
    this.deviceService.getFuelUsageChartData(obj).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true) {
        if (response.data.length) {
          data.push(['Months', 'Fuel']);

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
    let obj = { companyGuid: this.currentUser.userDetail.companyId, deviceGuid: this.generatordetailGuid };
    let data = [];
    this.spinner.show();
    this.deviceService.getEnergyUsageChartData(obj).subscribe(response => {

      if (response.isSuccess === true) {
        if (response.data.length) {
          data.push(['Months', 'Energy']);

          response.data.forEach(element => {
            data.push([element.month, parseFloat(element.value)]);
          });
          this.createHistoryChart('energyConsumption', data, 'Months', 'KWH','#5496d0');
          this.spinner.hide();
        } else {
          this.spinner.hide();
          this.chart.energyConsumption.dataTable = [];
        }
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

  createHistoryChart(key, data, hAxisTitle, vAxisTitle,color) {
    let height = this.chartHeight;
    if (key === 'soilNutritions') {
      height = 450
    }
    this.chart[key] = {
      chartType: 'ColumnChart',
      dataTable: data,
      options: {
        bar: { groupWidth: "25%" },
        colors: [color],
        height: height,
        width: this.chartWidth,
        legend: {position: 'none'},
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
          }
        }
      },
      formatters: this.headFormate
    };
  }

  getStompConfig() {

    this.deviceService.getStompConfig('LiveData').subscribe(response => {
      if (response.isSuccess) {
        this.stompConfiguration.url = response.data.url;
        this.stompConfiguration.headers.login = response.data.user;
        this.stompConfiguration.headers.passcode = response.data.password;
        this.stompConfiguration.headers.host = response.data.vhost;
        this.cpId = response.data.cpId;
        this.initStomp();
      }
    });
  }
  initStomp() {
    let config = this.stompConfiguration;
    this.stompService.config = config;
    this.stompService.initAndConnect();
    this.stompSubscribe();
  }
  public stompSubscribe() {
    if (this.subscribed) {
      return;
    }

    this.messages = this.stompService.subscribe('/topic/' + this.cpId + '-' + this.dataobj.uniqueId);
    this.subscription = this.messages.subscribe(this.on_next);
    this.subscribed = true;
  }
  public on_next = (message: Message) => {
    let obj: any = JSON.parse(message.body);
    if (obj.data.msgType === 'telemetry') {
      let reporting_data = obj.data.data.reporting
      this.isConnected = true;
      //--updating live data into statisatics--//
      this.deviceIsConnected = true;
      this.sensorData = reporting_data;
      //--updating live data into statisatics--//

      let dates = obj.data.data.time;
      let now = moment();
      if (obj.data.data.status != 'off') {
        this.options = {
          type: 'line',
          scales: {

            xAxes: [{
              type: 'realtime',
              time: {
                stepSize: 10
              },
              realtime: {
                duration: 90000,
                refresh: 7000,
                delay: 2000,
                onRefresh: function (chart: any) {
                  if (obj.data.data.status != 'on') {
                    chart.data.datasets.forEach(function (dataset: any) {

                      dataset.data.push({

                        x: now,

                        y: reporting_data[dataset.label]

                      });

                    });
                  }


                },

                // delay: 2000

              }

            }],
            yAxes: [{
              scaleLabel: {
                display: true,
                labelString: 'value'
              }
            }]

          },
          tooltips: {
            mode: 'nearest',
            intersect: false
          },
          hover: {
            mode: 'nearest',
            intersect: false
          }

        }
      }
      obj.data.data.time = now;
    } else if (obj.data.msgType === 'simulator') {
      if (obj.data.data.status === 'off') {
        this.deviceIsConnected = false;
      } else {
        this.deviceIsConnected = true;
      }
    }
  }
  createFormGroup() {
    this.imageForm = new FormGroup({
      files: new FormControl(null, [Validators.required])
    });
  }




  // For get TelemetryData
  getgenraterTelemetryData(templateGuid) {
    this.spinner.show();
    this.deviceService.getgenraterTelemetryData(templateGuid).subscribe(response => {
      if (response.isSuccess === true) {
        this.spinner.hide();
        let temp = [];
        response.data.forEach((element, i) => {
          var colorNames = Object.keys(this.chartColors);
          var colorName = colorNames[i % colorNames.length];
          var newColor = this.chartColors[colorName];
          var graphLabel = {
            label: element.text,
            backgroundColor: 'rgb(153, 102, 255)',
            borderColor: newColor,
            fill: false,
            cubicInterpolationMode: 'monotone',
            data: []
          }
          temp.push(graphLabel);
        });
        // response.data.forEach(element, i) => {

        // });
        this.datasets = temp;
        this.getStompConfig();
      } else {
        this._notificationService.add(new Notification('error', response.message));
      }
    }, error => {
      this.spinner.hide();
      this._notificationService.add(new Notification('error', error));
    });
  }

  getgenraterMedia(genraterGuid) {
    this.spinner.show();
    this.deviceService.getgenraterMedia(genraterGuid).subscribe(response => {
      if (response.isSuccess === true) {
        this.spinner.hide();
        this.getDeviceStatus(response.data.uniqueId)
        this.getgenraterTelemetryData(response.data.templateGuid);
        this.mediaFiles = response.data.mediaFiles
        this.dataobj = response.data
        if (response.data.image) {
          this.mediaUrl = this._notificationService.apiBaseUrl + response.data.image
        }
        else {
          this.mediaUrl = this._appConstant.noImg;
        }
      } else {
        this._notificationService.add(new Notification('error', response.message));
      }
    }, error => {
      this.spinner.hide();
      this._notificationService.add(new Notification('error', error));
    });
  }

  downloadPdf(pdfUrls: string, pdfNames: string) {

    const pdfUrl = this._notificationService.apiBaseUrl + pdfUrls;
    const pdfName = pdfNames;
    FileSaver.saveAs(pdfUrl, pdfName);
  }

  handleImageInput(event) {
    const fileList: FileList = event.target.files;
    for (let x = 0; x < fileList.length; x++) {
      if (event.target.files[x]) {
        let fileType = fileList.item(x).name.split('.');
        let imagesTypes = ['doc', 'DOC', 'docx', 'DOCX', 'pdf', 'PDF'];
        if (imagesTypes.indexOf(fileType[fileType.length - 1]) !== -1) {
          this.selectedFiles.push(event.target.files[x]);
        } else {
          this.imageForm.reset();
          this.checkSubmitStatus = false;
          this.selectedFiles = [];
          this.MesageAlertDataModel = {
            title: "Media Files",
            message: "Invalid File Type.",
            message2: "",
            okButtonName: "OK",
          };
          const dialogRef = this.dialog.open(MessageDialogComponent, {
            width: '400px',
            height: 'auto',
            data: this.MesageAlertDataModel,
            disableClose: false
          });
          return;
        }
      }

    }


    if (event.target.files && event.target.files[0]) {
      var reader = new FileReader();
      reader.readAsDataURL(event.target.files[0]);
      reader.onload = (innerEvent: any) => {
        //this.fileUrl = innerEvent.target.result;
      }
    }
  }

  uploadMedia() {
    this.checkSubmitStatus = true;
    if (this.imageForm.status === "VALID") {
      this.deviceService.UploadmediaGenrator(this.selectedFiles, this.generatordetailGuid).subscribe(response => {
        this.refresh();
        this.getgenraterDetail(this.generatordetailGuid);
        this.getgenraterMedia(this.generatordetailGuid);
      })
    }
  }

  deleteModel(GeneratorModel: any) {
    this.deleteAlertDataModel = {
      title: "Delete File",
      message: this._appConstant.msgConfirm.replace('modulename', "File"),
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
        this.deleteFiles(GeneratorModel);
      }
    });
  }

  deleteFiles(guid) {
    this.spinner.show();
    this.deviceService.deleteFiles(this.generatordetailGuid, guid).subscribe(response => {
      this.spinner.hide();
      if (response.isSuccess === true) {
        this._notificationService.add(new Notification('success', this._appConstant.msgDeleted.replace("modulename", "File")));
        this.getgenraterDetail(this.generatordetailGuid);
        this.getgenraterMedia(this.generatordetailGuid);
      }
      else {
        this._notificationService.add(new Notification('error', response.message));
      }
    }, error => {
      this.spinner.hide();
      this._notificationService.add(new Notification('error', error));
    });
  }

  refresh() {
    this.imageForm.reset();
    this.respondShow = false;
    this.checkSubmitStatus = false;
    this.selectedFiles = [];
  }
}
