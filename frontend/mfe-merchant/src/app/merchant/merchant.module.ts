import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { MerchantRoutingModule } from './merchant-routing.module';

import { StoreCreationComponent } from './store-creation/store-creation.component';
import { StoreListComponent } from './store-list/store-list.component';
import { StoreSettingsComponent } from './store-settings/store-settings.component';
import { StoreViewComponent } from './store-view/store-view.component';

@NgModule({
  declarations: [
    StoreCreationComponent,
    StoreListComponent,
    StoreSettingsComponent,
    StoreViewComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    MerchantRoutingModule
  ]
})
export class MerchantModule { }
