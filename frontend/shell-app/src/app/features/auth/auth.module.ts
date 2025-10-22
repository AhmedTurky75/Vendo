import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { AuthRoutingModule } from './auth-routing.module';

import { AdminLoginComponent } from './admin-login/admin-login.component';
import { MerchantLoginComponent } from './merchant-login/merchant-login.component';
import { CustomerLoginComponent } from './customer-login/customer-login.component';

@NgModule({
  declarations: [
    AdminLoginComponent,
    MerchantLoginComponent,
    CustomerLoginComponent
  ],
  imports: [
    SharedModule,
    AuthRoutingModule
  ]
})
export class AuthModule { }
