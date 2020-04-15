import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'
import { FormControl, FormGroup, Validators } from '@angular/forms'
import { NgxSpinnerService } from 'ngx-spinner'
import { DeviceService, NotificationService, LookupService } from 'app/services';
import { Notification } from 'app/services/notification/notification.service';
import { AppConstant } from "../../../app.constants";
import { Guid } from "guid-typescript";
import { Observable, forkJoin } from 'rxjs';
import { Location } from '@angular/common';


export interface DeviceTypeList {
	id: number;
	type: string;
}
export interface StatusList {
	id: boolean;
	status: string;
}
@Component({
	selector: 'app-location-add',
	templateUrl: './location-add.component.html',
	styleUrls: ['./location-add.component.css']
})


export class LocationAddComponent implements OnInit {
	currentUser: any;
	fileUrl: any;
	fileName = '';
	fileToUpload: any = null;
	status;
	moduleName = "Add Location";
	parentDeviceObject: any = {};
	deviceObject = {};
	deviceGuid = '';
	parentDeviceGuid = '';
	isEdit = false;
	locationForm: FormGroup;
	checkSubmitStatus = false;
	templateList = [];
	tagList = [];
	countryList = [];
	stateList = [];
	statusList: StatusList[] = [
		{
			id: true,
			status: 'Active'
		},
		{
			id: false,
			status: 'In-active'
		}

	];
	locationGuid: any;
	locationObject = {};
	constructor(
		private router: Router,
		private _notificationService: NotificationService,
		private activatedRoute: ActivatedRoute,
		private spinner: NgxSpinnerService,
		private deviceService: DeviceService,
		private lookupService: LookupService,
		public _appConstant: AppConstant,
		public location: Location,
	) {
		this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
		this.activatedRoute.params.subscribe(params => {
			// set data for parent device
			if (params.locationGuid != null) {
				this.getlocationDetail(params.locationGuid);
				this.locationGuid = params.locationGuid;
				this.moduleName = "Edit Location";
				this.isEdit = true;
			} else {
				this.locationObject = { name: '', description: '', countryGuid: '', stateGuid: '', latitude: '', longitude: '', city: '', zipcode: '', address: '', address2: '' }
			}
		});
	}

	// before view init
	ngOnInit() {
		this.createFormGroup();
		this.getCountryLookup();
	}



	createFormGroup() {
		this.locationForm = new FormGroup({
			guid: new FormControl(''),
			name: new FormControl('', [Validators.required]),
			description: new FormControl(''),
			countryGuid: new FormControl(null, [Validators.required]),
			stateGuid: new FormControl(null, [Validators.required]),
			latitude: new FormControl('', [Validators.required,Validators.pattern('^(\\+|-)?(?:90(?:(?:\\.0{1,6})?)|(?:[0-9]|[1-8][0-9])(?:(?:\\.[0-9]{1,6})?))$')]),
			longitude: new FormControl('', [Validators.required,Validators.pattern('^(\\+|-)?(?:180(?:(?:\\.0{1,6})?)|(?:[0-9]|[1-9][0-9]|1[0-7][0-9])(?:(?:\\.[0-9]{1,6})?))$')]),
			city: new FormControl('', [Validators.required]),
			zipcode: new FormControl('', [Validators.required,Validators.pattern('^[0-9]*$')]),
			address: new FormControl(''),
			address2: new FormControl(''),
			parentEntityGuid: new FormControl(''),
			companyGuid: new FormControl('')
		});
	}


	/**
	 * Get all the data related to parent device using forkjoin (Combine services)
	 * 
	 * @param deviceGuid 
	 * 
	 */
	getlocationDetail(locationGuid) {

		this.spinner.show();
		this.deviceService.getLocationDetails(locationGuid).subscribe(response => {
			if (response.isSuccess === true) {
				this.spinner.hide();
				this.getstate(response.data.countryGuid)
				this.locationObject = response.data;
			} else {
				this._notificationService.add(new Notification('error', response.message));
			}
		}, error => {
			this.spinner.hide();
			this._notificationService.add(new Notification('error', error));
		});
	}

	/**
	 * set parent device details
	 * @param response 
	 */
	setParentDeviceDetails(response) {
		if (response.isSuccess === true) {
			this.parentDeviceObject = response.data;
			//Get tags lookup once parent device data is fetched
			//this.getTagsLookup();
		} else {
			this._notificationService.add(new Notification('error', response.message));
		}

	}

	/**
	 * set template lookup
	 * only gateway supported template
	 *  @param response 
	 */
	setTemplateLookup(response) {
		if (response.isSuccess === true) {
			this.templateList = response['data'];
		} else {
			this._notificationService.add(new Notification('error', response.message));
		}
	}

	/**
	 * Get tags lookup once parent device data is fetched
	 */
	getCountryLookup() {
		this.lookupService.getcountryList().
			subscribe(response => {
				if (response.isSuccess === true) {
					this.countryList = response.data;
				} else {
					this._notificationService.add(new Notification('error', response.message));
				}
			}, error => {
				this.spinner.hide();
				this._notificationService.add(new Notification('error', error));
			})
	}

	getstate(event) {
		this.spinner.show();
		this.stateList = [];
		this.locationForm.controls['stateGuid'].setValue(null, { emitEvent: true })
		if (event) {
			this.lookupService.getcitylist(event).
				subscribe(response => {
					if (response.isSuccess === true) {
						this.stateList = response.data;
						this.spinner.hide();
					} else {
						this.spinner.hide();
						this._notificationService.add(new Notification('error', response.message));
					}
				}, error => {
					this.spinner.hide();
					this._notificationService.add(new Notification('error', error));
				})

		}
	}

	log(obj) {
	}


	/**
	 * Find a value from the look up data
	 * 
	 * @param obj 
	 * 
	 * @param findByvalue 
	 * 
	 */
	getIndexByValue(obj, findByvalue) {
		let index = obj.findIndex(
			(tmpl) => { return (tmpl.value == findByvalue.toUpperCase()) }
		);
		if (index > -1) return obj[index].text;
		return;
	}


	/**
	 * Add device under gateway
	 * only gateway supported device
	 */
	addLocation() {
		this.checkSubmitStatus = true;
		this.locationForm.get('guid').setValue('00000000-0000-0000-0000-000000000000');
		if (this.locationForm.status === "VALID") {
			if (this.isEdit) {
				this.locationForm.registerControl("guid", new FormControl(''));
				this.locationForm.patchValue({ "guid": this.locationGuid });
			}
			this.spinner.show();
			let currentUser = JSON.parse(localStorage.getItem('currentUser'));
			this.locationForm.get('parentEntityGuid').setValue(currentUser.userDetail.entityGuid);
			this.locationForm.get('companyGuid').setValue(currentUser.userDetail.companyId);
			this.deviceService.addUpdateLocation(this.locationForm.value).subscribe(response => {
				if (response.isSuccess === true) {
					this.spinner.hide();
					if (this.isEdit) {
						this._notificationService.add(new Notification('success', "Location has been updated successfully."));
					} else {
						this._notificationService.add(new Notification('success', "Location has been added successfully."));
					}
					this.location.back();
					this.router.navigate(['locations']);
				} else {
					this.spinner.hide();
					this._notificationService.add(new Notification('error', response.message));
				}
			})
		}
	}


}
