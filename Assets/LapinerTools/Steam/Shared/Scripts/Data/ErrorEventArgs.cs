using UnityEngine;
using System.Collections;

using Steamworks;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// Error event arguments. This class also contains human readable error messages for various error scenarios.
	/// </summary>
	public class ErrorEventArgs : EventArgsBase
	{
		public const string ERROR_MSG_STEAM_NOT_INIT = "Steam must be running!";
		public const string ERROR_MSG_WORKSHOP_LEGAL_AGREEMENT = "Please accept Steam Workshop legal agreement first!";
		public static string ERROR_MSG(EResult p_result)
		{
			switch (p_result)
			{
				case EResult.k_EResultFail: return "Generic failure!";
				case EResult.k_EResultNoConnection: return "No/failed network connection!";
				case EResult.k_EResultInvalidPassword: return "Password/ticket is invalid!";
				case EResult.k_EResultLoggedInElsewhere: return "Same user logged in elsewhere!";
				case EResult.k_EResultInvalidProtocolVer: return "Protocol version is incorrect!";
				case EResult.k_EResultInvalidParam: return "A parameter is incorrect!";
				case EResult.k_EResultFileNotFound: return "File was not found!";
				case EResult.k_EResultBusy: return "Called method busy - action not taken!";
				case EResult.k_EResultInvalidState: return "Called object was in an invalid state!";
				case EResult.k_EResultInvalidName: return "Name is invalid!";
				case EResult.k_EResultInvalidEmail: return "E-mail is invalid!";
				case EResult.k_EResultDuplicateName: return "Name is not unique!";
				case EResult.k_EResultAccessDenied: return "Access is denied!";
				case EResult.k_EResultTimeout: return "Operation timed out!";
				case EResult.k_EResultBanned: return "VAC2 banned!";
				case EResult.k_EResultAccountNotFound: return "Account not found!";
				case EResult.k_EResultInvalidSteamID: return "SteamID is invalid!";
				case EResult.k_EResultServiceUnavailable: return "The requested service is currently unavailable!";
				case EResult.k_EResultNotLoggedOn: return "The user is not logged on!";
				case EResult.k_EResultPending: return "Request is pending (may be in process, or waiting on third party)!";
				case EResult.k_EResultEncryptionFailure: return "Encryption or decryption failed!";
				case EResult.k_EResultInsufficientPrivilege: return "Insufficient privilege!";
				case EResult.k_EResultLimitExceeded: return "Limit exceeded!";
				case EResult.k_EResultRevoked: return "Access has been revoked!";
				case EResult.k_EResultExpired: return "License/guest pass is expired!";
				case EResult.k_EResultAlreadyRedeemed: return "Guest pass has already been redeemed by account, cannot be used again!";
				case EResult.k_EResultDuplicateRequest: return "The request is a duplicate and the action has already occurred in the past, ignored this time!";
				case EResult.k_EResultAlreadyOwned: return "All the games in this guest pass redemption request are already owned!";
				case EResult.k_EResultIPNotFound: return "IP address not found!";
				case EResult.k_EResultPersistFailed: return "Failed to write change to the data store!";
				case EResult.k_EResultLockingFailed: return "Failed to acquire access lock for this operation!";
				case EResult.k_EResultLogonSessionReplaced: return "Logon session replaced!";
				case EResult.k_EResultConnectFailed: return "Connect failed!";
				case EResult.k_EResultHandshakeFailed:return "Handshake failed!";
				case EResult.k_EResultIOFailure: return "IO failure!";
				case EResult.k_EResultRemoteDisconnect: return "Remote disconnect!";
				case EResult.k_EResultShoppingCartNotFound: return "Failed to find the shopping cart requested!";
				case EResult.k_EResultBlocked: return "A user didn't allow it!";
				case EResult.k_EResultIgnored: return "Target is ignoring sender!";
				case EResult.k_EResultNoMatch: return "Nothing matching the request found!";
				case EResult.k_EResultAccountDisabled: return "Account disabled!";
				case EResult.k_EResultServiceReadOnly: return "This service is not accepting content changes right now!";
				case EResult.k_EResultAccountNotFeatured: return "Account doesn't have value, so this feature isn't available!";
				case EResult.k_EResultAdministratorOK: return "Allowed to take this action, but only because requester is admin!";
				case EResult.k_EResultContentVersion: return "A Version mismatch in content transmitted within the Steam protocol!";
				case EResult.k_EResultTryAnotherCM: return "The current CM can't service the user making a request, user should try another!";
				case EResult.k_EResultPasswordRequiredToKickSession: return "You are already logged in elsewhere, this cached credential login has failed!";
				case EResult.k_EResultAlreadyLoggedInElsewhere: return "You are already logged in elsewhere, you must wait!";
				case EResult.k_EResultSuspended: return "Long running operation (content download) suspended/paused!";
				case EResult.k_EResultCancelled: return "Operation canceled!";
				case EResult.k_EResultDataCorruption: return "Operation canceled, because data is ill formed or unrecoverable!";
				case EResult.k_EResultDiskFull: return "Operation canceled, because not enough disk space!";
				case EResult.k_EResultRemoteCallFailed: return "A remote call or IPC call failed!";
				case EResult.k_EResultPasswordUnset: return "Password could not be verified as it's unset server side!";
				case EResult.k_EResultExternalAccountUnlinked: return "External account (PSN, Facebook...) is not linked to a Steam account!";
				case EResult.k_EResultPSNTicketInvalid: return "PSN ticket was invalid!";
				case EResult.k_EResultExternalAccountAlreadyLinked: return "External account (PSN, Facebook...) is already linked to some other account, must explicitly request to replace/delete the link first!";
				case EResult.k_EResultRemoteFileConflict: return "The sync cannot resume due to a conflict between the local and remote files!";
				case EResult.k_EResultIllegalPassword: return "The requested new password is not legal!";
				case EResult.k_EResultSameAsPreviousValue: return "New value is the same as the old one!";
				case EResult.k_EResultAccountLogonDenied: return "Account login denied due to 2nd factor authentication failure!";
				case EResult.k_EResultCannotUseOldPassword: return "The requested new password is not legal!";
				case EResult.k_EResultInvalidLoginAuthCode: return "Account login denied due to auth code invalid!";
				case EResult.k_EResultAccountLogonDeniedNoMail: return "Account login denied due to 2nd factor auth failure - and no mail has been sent!";
				case EResult.k_EResultHardwareNotCapableOfIPT: return "Hardware not capable of IPT!";
				case EResult.k_EResultIPTInitError: return "IPT init error!";
				case EResult.k_EResultParentalControlRestricted: return "Operation failed due to parental control restrictions for current user!";
				case EResult.k_EResultFacebookQueryError: return "Facebook query returned an error!";
				case EResult.k_EResultExpiredLoginAuthCode: return "Account login denied due to auth code expired!";
				case EResult.k_EResultIPLoginRestrictionFailed: return "IP login restriction failed!";
				case EResult.k_EResultAccountLockedDown: return "Account locked down!";
				case EResult.k_EResultAccountLogonDeniedVerifiedEmailRequired: return "Account logon denied, verified e-mail required!";
				case EResult.k_EResultNoMatchingURL: return "No matching URL!";
				case EResult.k_EResultBadResponse: return "Parse failure, missing field, etc!";
				case EResult.k_EResultRequirePasswordReEntry: return "The user cannot complete the action until they re-enter their password!";
				case EResult.k_EResultValueOutOfRange: return "The value entered is outside the acceptable range!";
				case EResult.k_EResultUnexpectedError: return "Something happened that we didn't expect to ever happen!";
				case EResult.k_EResultDisabled: return "The requested service has been configured to be unavailable!";
				case EResult.k_EResultInvalidCEGSubmission: return "The set of files submitted to the CEG server are not valid!";
				case EResult.k_EResultRestrictedDevice: return "The device being used is not allowed to perform this action!";
				case EResult.k_EResultRegionLocked: return "The action could not be complete because it is region restricted!";
				case EResult.k_EResultRateLimitExceeded: return "Temporary rate limit exceeded, try again later!";
				case EResult.k_EResultAccountLoginDeniedNeedTwoFactor: return "Need two-factor code to login!";
				case EResult.k_EResultItemDeleted: return "The thing we're trying to access has been deleted!";
				case EResult.k_EResultAccountLoginDeniedThrottle: return "Login attempt failed, try to throttle response to possible attacker!";
				case EResult.k_EResultTwoFactorCodeMismatch: return "Two factor code mismatch!";
				case EResult.k_EResultTwoFactorActivationCodeMismatch: return "Activation code for two-factor didn't match!";
				case EResult.k_EResultAccountAssociatedToMultiplePartners: return "Account has been associated with multiple partners!";
				case EResult.k_EResultNotModified: return "Data not modified!";
				case EResult.k_EResultNoMobileDevice: return "The account does not have a mobile device associated with it!";
				case EResult.k_EResultTimeNotSynced: return "The time presented is out of range or tolerance!";
				case EResult.k_EResultSmsCodeFailed: return "SMS code failure (no match, none pending, etc.)!";
				case EResult.k_EResultAccountLimitExceeded: return "Too many accounts access this resource!";
				case EResult.k_EResultAccountActivityLimitExceeded: return "Too many changes to this account!";
				case EResult.k_EResultPhoneActivityLimitExceeded: return "Too many changes to this phone!";
				case EResult.k_EResultRefundToWallet: return "Cannot refund to payment method, must use wallet!";
				case EResult.k_EResultEmailSendFailure: return "Cannot send an e-mail!";
				case EResult.k_EResultNotSettled: return "Can't perform operation till payment has settled!";
				default: return "Internal error!";
			}
		}

		public static ErrorEventArgs CreateSteamNotInit() { return new ErrorEventArgs(ERROR_MSG_STEAM_NOT_INIT); }
		public static ErrorEventArgs CreateWorkshopLegalAgreement() { return new ErrorEventArgs(ERROR_MSG_WORKSHOP_LEGAL_AGREEMENT); }
		public static ErrorEventArgs Create(EResult p_result) { return new ErrorEventArgs(ERROR_MSG(p_result)); }

		public ErrorEventArgs()
		{
			IsError = true;
		}

		public ErrorEventArgs(string p_errorMessage)
		{
			IsError = true;
			ErrorMessage = p_errorMessage;
		}
	}
}
