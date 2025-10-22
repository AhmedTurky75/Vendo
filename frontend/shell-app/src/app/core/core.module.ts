import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { OAuthModule } from 'angular-oauth2-oidc';
import { AuthInterceptor } from './interceptors/auth.interceptor';

/**
 * CoreModule
 *
 * This module contains singleton services and configurations
 * that should only be imported once in the AppModule.
 *
 * Includes:
 * - OAuth2/OIDC configuration
 * - HTTP interceptors
 * - Global services
 */
@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    OAuthModule.forRoot({
      resourceServer: {
        allowedUrls: ['http://localhost:5001/api'],
        sendAccessToken: true
      }
    })
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ]
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error('CoreModule is already loaded. Import it in the AppModule only');
    }
  }
}
