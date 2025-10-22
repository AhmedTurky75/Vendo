import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLoginComponent } from './admin-login/admin-login.component';
import { MerchantLoginComponent } from './merchant-login/merchant-login.component';
import { CustomerLoginComponent } from './customer-login/customer-login.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { AuthCallbackComponent } from './auth-callback/auth-callback.component';
import { SignoutCallbackComponent } from './signout-callback/signout-callback.component';

const routes: Routes = [
  {
    path: 'login',
    children: [
      {
        path: 'admin',
        component: AdminLoginComponent
      },
      {
        path: 'merchant',
        component: MerchantLoginComponent
      },
      {
        path: 'customer',
        component: CustomerLoginComponent
      },
      {
        path: '',
        redirectTo: 'customer',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: 'auth',
    children: [
      {
        path: 'callback',
        component: AuthCallbackComponent
      },
      {
        path: 'signout-callback',
        component: SignoutCallbackComponent
      }
    ]
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordComponent
  },
  {
    path: 'reset-password',
    component: ResetPasswordComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
