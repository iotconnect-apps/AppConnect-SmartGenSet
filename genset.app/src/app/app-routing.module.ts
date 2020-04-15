import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'

import { SelectivePreloadingStrategy } from './selective-preloading-strategy'
import { PageNotFoundComponent } from './page-not-found.component'
import {
	RolesListComponent, RolesAddComponent,HomeComponent, UserListComponent, UserAddComponent, DashboardComponent,
  LoginComponent, RegisterComponent, MyProfileComponent, ResetpasswordComponent, SettingsComponent, AlertsComponent,
	ChangePasswordComponent,AdminLoginComponent, SubscribersListComponent, HardwareListComponent, HardwareAddComponent, UserAdminListComponent, AdminUserAddComponent, AdminDashboardComponent, SubscriberDetailComponent,GeneratorAddComponent,
	BulkuploadAddComponent,LocationListComponent, GeneratorComponent,GeneratorListComponent,LocationAddComponent,LocationDetailComponent,GeneratorDetailComponent
} from './components/index'


import { AuthService,AdminAuthGuired } from './services/index'



const appRoutes: Routes = [
	{
		path: 'admin',
		children: [
			{
				path: '',
				component: AdminLoginComponent
			},
			{
				path: 'dashboard',
				component: AdminDashboardComponent,
				canActivate: [AuthService]
			},
			{
        		path: 'subscriber/:email/:productCode/:companyId',
				component: SubscriberDetailComponent,
				canActivate: [AuthService]
			},
			{
				path: 'subscribers',
				component: SubscribersListComponent,
				canActivate: [AuthService]
			},
			{
				path: 'hardwarekits',
				component: HardwareListComponent,
				canActivate: [AuthService]
			},
			{
				path: 'hardwarekit/add',
				component: HardwareAddComponent,
				canActivate: [AuthService]
			},
			{
				path: 'hardwarekit/:hardwarekitGuid',
				component: HardwareAddComponent,
				canActivate: [AuthService]
			},
			{
				path: 'users',
				component: UserAdminListComponent,
				canActivate: [AuthService]
			},
			{
				path: 'user/add',
				component: AdminUserAddComponent,
				canActivate: [AuthService]
			},
			{
				path: 'user/:userGuid',
				component: AdminUserAddComponent,
				canActivate: [AuthService]
			},
			{
        path: 'bulkupload',
				component: BulkuploadAddComponent,
				canActivate: [AuthService]
			}
		]
	},
	{
		path: '',
		component: HomeComponent
	},
	{
		path: 'login',
		component: LoginComponent
	},
	{
		path: 'register',
		component: RegisterComponent
	},
	//App routes goes here 
	{
		path: 'my-profile',
		component: MyProfileComponent,
		// canActivate: [AuthService]
	},
	{
		path: 'change-password',
		component: ChangePasswordComponent,
		// canActivate: [AuthService]
	},
	{
		path: 'dashboard',
		component: DashboardComponent,
		canActivate: [AdminAuthGuired]
	},
	{
		path: 'locations',
		component: LocationListComponent,
		canActivate: [AdminAuthGuired]
	},
	{
		path: 'location/add',
		component: LocationAddComponent,
    canActivate: [AdminAuthGuired]
	},
	{
		path: 'location/:locationGuid',
		component: LocationAddComponent,
    canActivate: [AdminAuthGuired]
	},
	{
		path: 'location-detail/:locationGuid',
		component: LocationDetailComponent,
    canActivate: [AdminAuthGuired]
	},
	{
		path: 'generator',
		component: GeneratorComponent,
		canActivate: [AdminAuthGuired]
	},
	{
		path: 'generator/add',
		component: GeneratorAddComponent,
    canActivate: [AdminAuthGuired]
	},
	{
		path: 'generator/:generatorGuid',
		component: GeneratorAddComponent,
    canActivate: [AdminAuthGuired]
	},
	{
		path: 'generatordetails/:generatordetailGuid',
		component: GeneratorDetailComponent,
		canActivate: [AdminAuthGuired]
	},
    {
		path: 'user/:userGuid',
		component: UserAddComponent,
		canActivate: [AdminAuthGuired]
	}, {
		path: 'user/add',
		component: UserAddComponent,
		canActivate: [AdminAuthGuired]
	}, {
		path: 'users',
		component: UserListComponent,
		canActivate: [AdminAuthGuired]
	},
	{
		path: 'roles/:deviceGuid',
		component: RolesAddComponent,
		canActivate: [AdminAuthGuired]
	}, {
		path: 'roles',
		component: RolesListComponent,
		canActivate: [AdminAuthGuired]
	},
	{
		path: 'generators',
		component: GeneratorListComponent,
    canActivate: [AdminAuthGuired]
  },
  {
    path: 'alerts',
    component: AlertsComponent,
    canActivate: [AdminAuthGuired]
  },
	{
		path: '**',
		component: PageNotFoundComponent
	}
];

@NgModule({
	imports: [
		RouterModule.forRoot(
			appRoutes, {
			preloadingStrategy: SelectivePreloadingStrategy
		}
		)
	],
	exports: [
		RouterModule
	],
	providers: [
		SelectivePreloadingStrategy
	]
})

export class AppRoutingModule { }
