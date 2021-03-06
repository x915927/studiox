﻿import { Component, Injector, ElementRef, AfterViewInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AppComponentBase } from '@shared/app-component-base';
import { LoginService, ExternalLoginProvider } from './login.service';
import { accountModuleAnimation } from '@shared/animations/routerTransition';
import { StudioXSessionService } from '@studiox/session/studiox-session.service';

@Component({
    templateUrl: './login.component.html',
    styleUrls: [
        './login.component.less'
    ],
    animations: [accountModuleAnimation()] 
})
export class LoginComponent extends AppComponentBase {

    @ViewChild('cardBody') cardBody: ElementRef;

    submitting: boolean = false;

    constructor(
        injector: Injector,
        public loginService: LoginService,
        private _router: Router,
        private _sessionService: StudioXSessionService
    ) {
        super(injector);
    }

    ngAfterViewInit(): void {
        ($ as any).AdminBSB.input.activate($(this.cardBody.nativeElement));
        $(this.cardBody.nativeElement).find('input:first').focus();
    }

    get multiTenancySideIsTeanant(): boolean {
        return this._sessionService.tenantId > 0;
    }

    get isSelfRegistrationAllowed(): boolean {
        if (!this._sessionService.tenantId) {
            return false;
        }

        return true;
    }

    login(): void {
        this.submitting = true;
        this.loginService.authenticate(
            () => this.submitting = false
        );
    }

    externalLogin(provider: ExternalLoginProvider) {
        this.loginService.externalAuthenticate(provider);
    }
}