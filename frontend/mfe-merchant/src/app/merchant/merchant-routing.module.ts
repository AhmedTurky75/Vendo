import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StoreCreationComponent } from './store-creation/store-creation.component';
import { StoreListComponent } from './store-list/store-list.component';
import { StoreSettingsComponent } from './store-settings/store-settings.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'onboarding',
        component: StoreCreationComponent
      },
      {
        path: 'dashboard',
        component: StoreListComponent
      },
      {
        path: 'stores/:id/settings',
        component: StoreSettingsComponent
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MerchantRoutingModule { }
