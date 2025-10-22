import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLoginComponent } from './admin-login/admin-login.component';
import { MerchantLoginComponent } from './merchant-login/merchant-login.component';
import { CustomerLoginComponent } from './customer-login/customer-login.component';

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
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
