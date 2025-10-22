import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { AuthRoutingModule } from './auth-routing.module';

import { AdminLoginComponent } from './admin-login/admin-login.component';
import { MerchantLoginComponent } from './merchant-login/merchant-login.component';
import { CustomerLoginComponent } from './customer-login/customer-login.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';

@NgModule({
  declarations: [],
  imports: [
    SharedModule,
    AuthRoutingModule,
    AdminLoginComponent,
    MerchantLoginComponent,
    CustomerLoginComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent
  ]
})
export class AuthModule { }
