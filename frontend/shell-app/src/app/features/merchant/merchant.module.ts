import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { MerchantRoutingModule } from './merchant-routing.module';

import { StoreCreationComponent } from './store-creation/store-creation.component';
import { StoreListComponent } from './store-list/store-list.component';
import { StoreSettingsComponent } from './store-settings/store-settings.component';

@NgModule({
  declarations: [
    StoreCreationComponent,
    StoreListComponent,
    StoreSettingsComponent
  ],
  imports: [
    SharedModule,
    MerchantRoutingModule
  ]
})
export class MerchantModule { }
