import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { AuthRoutingModule } from './auth-routing.module';

import { AdminLoginComponent } from './admin-login/admin-login.component';
import { MerchantLoginComponent } from './merchant-login/merchant-login.component';
import { CustomerLoginComponent } from './customer-login/customer-login.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { AuthCallbackComponent } from './auth-callback/auth-callback.component';
import { SignoutCallbackComponent } from './signout-callback/signout-callback.component';

@NgModule({
  declarations: [
    AdminLoginComponent,
    MerchantLoginComponent,
    CustomerLoginComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    AuthCallbackComponent,
    SignoutCallbackComponent
  ],
  imports: [
    SharedModule,
    AuthRoutingModule
  ]
})
export class AuthModule { }
