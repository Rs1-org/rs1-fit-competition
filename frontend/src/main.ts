import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { importProvidersFrom } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { AppModule } from './app/app.module';

registerLocaleData(localeBs);

bootstrapApplication(App, {
  providers: [importProvidersFrom(AppModule)],
}).catch((err: unknown) => console.error(err));