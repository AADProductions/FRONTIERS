using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.GUI;

namespace Frontiers
{
		public class Moneylenders : Manager
		{
				public List <Loan> Loans = new List <Loan>();

				#region current offer

				public string CurrentOfferOrganizationName = "Moneylender";
				public double CurrentOfferDailyInterestRate = 0;
				public int CurrentOfferPrincipal = 0;
				public WICurrencyType CurrentOfferCurrencyType = WICurrencyType.None;
				public List <MobileReference> CurrentOfferCollateral = new List <MobileReference>();
				public List <string> CurrentOfferCollateralReputation = new List <string>();

				public void ResetCurrentOffer()
				{
						CurrentOfferDailyInterestRate = 0;
						CurrentOfferPrincipal = 0;
						CurrentOfferCollateral.Clear();
						CurrentOfferCurrencyType = WICurrencyType.None;
						CurrentOfferCollateralReputation.Clear();
				}

				public void AcceptCurrentOffer()
				{
						Debug.Log("Accepting loan offer");
						Loan newLoan = null;
						if (TakeOutLoan(
								 CurrentOfferOrganizationName,
								 CurrentOfferPrincipal,
								 CurrentOfferDailyInterestRate,
								 CurrentOfferCollateral,
								 CurrentOfferCollateralReputation,
								 CurrentOfferCurrencyType,
								 out newLoan)) {
								Player.Local.Inventory.InventoryBank.Add(newLoan.InitialPrincipal, newLoan.CurrencyType);
								GUIManager.PostInfo("You have taken out a loan with " + CurrentOfferOrganizationName);
						}
				}

				public void AddCollateralToCurrentOffer(string locationPath)
				{
						if (!string.IsNullOrEmpty(locationPath)) {
								MobileReference collateral = new MobileReference(locationPath);
								CurrentOfferCollateral.SafeAdd(collateral);
						}
				}

				public void AddCollateralReputationToCurrentOffer(string characterName)
				{
						if (!string.IsNullOrEmpty(characterName)) {
								CurrentOfferCollateralReputation.SafeAdd(characterName);
						}
				}

				public void SetCurrentOfferOrganizationName(string organizationName)
				{
						if (!string.IsNullOrEmpty(organizationName)) {
								CurrentOfferOrganizationName = organizationName;
						}
				}

				public void SetCurrentOfferDailyInterestRate(double interestRate)
				{
						if (interestRate >= 0) {
								CurrentOfferDailyInterestRate = interestRate;
						}
				}

				public void SetCurrentOfferPrincipal(int principal)
				{
						if (principal > 0) {
								CurrentOfferPrincipal = principal;
						}
				}

				public void AddToCurrentOfferPrincipal(int principal)
				{
						if (principal > 0) {
								CurrentOfferPrincipal += principal;
						}
				}

				public void SetCurrentOfferCurrencyType(WICurrencyType currencyType)
				{
						if (currencyType != WICurrencyType.None) {
								CurrentOfferCurrencyType = currencyType;
						}
				}

				public bool CanMakeCurrentOffer {
						get {
								return (!string.IsNullOrEmpty(CurrentOfferOrganizationName)
								&& CurrentOfferDailyInterestRate >= 0
								&& CurrentOfferPrincipal > 0
								&& (CurrentOfferCollateral.Count > 0 || CurrentOfferCollateralReputation.Count > 0)
								&& CurrentOfferCurrencyType != WICurrencyType.None
								&& !HasOutstandingLoan(CurrentOfferOrganizationName));
						}
				}

				#endregion

				public static Moneylenders Get;

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
				}

				public override void OnModsLoadStart()
				{
						Mods.Get.Runtime.LoadAvailableMods <Loan>(Loans, "Loan");
				}

				public bool TakeOutLoan(
						string organizationName,
						int principal,
						double dailyInterestRate,
						List <MobileReference> collateralStructures,
						List <string> collateralReputation,
						WICurrencyType currencyType,
						out Loan newLoan)
				{

						if (HasOutstandingLoan(organizationName, out newLoan)) {
								//can only have one loan per organization at a time
								return false;
						}

						newLoan = new Loan();
						newLoan.CurrencyType = currencyType;
						newLoan.OrganizationName = organizationName;
						newLoan.LoanNumber = GetNumLoansFromOrganization(organizationName) + 1;
						newLoan.DayCreated = WorldClock.DaysSinceBeginningOfTime;
						newLoan.DayLastPaymentMade = WorldClock.DaysSinceBeginningOfTime;
						newLoan.DayPaidOff = -1;
						newLoan.InitialPrincipal = principal;
						newLoan.CurrentPrincipal = principal;
						newLoan.DailyInterestRate = dailyInterestRate;
						newLoan.CollateralStructures.AddRange(collateralStructures);
						newLoan.CollateralReputation.AddRange(collateralReputation);
						newLoan.Name = organizationName + "-" + newLoan.LoanNumber.ToString();
						Mods.Get.Runtime.SaveMod <Loan>(newLoan, "Loan", newLoan.Name);
						Loans.Add(newLoan);
						return true;
				}

				public void RepayLoan(string organizationName)
				{
						Loan outstandingLoan = null;
						if (HasOutstandingLoan(organizationName, out outstandingLoan)
						 && Player.Local.Inventory.InventoryBank.CanAfford(outstandingLoan.AmountOwed, outstandingLoan.CurrencyType)) {
								MakePayment(outstandingLoan, outstandingLoan.AmountOwed);
						}
				}

				public void MakePayment(string organizationName, int baseCurrenyAmount)
				{
						Loan outstandingLoan = null;
						if (HasOutstandingLoan(organizationName, out outstandingLoan)
						 && Player.Local.Inventory.InventoryBank.CanAfford(outstandingLoan.AmountOwed, outstandingLoan.CurrencyType)) {
								MakePayment(outstandingLoan, baseCurrenyAmount);
						}
				}

				public void MakePayment(Loan outstandingLoan, int paymentAmount)
				{
						int amountPaid = 0;
						Player.Local.Inventory.InventoryBank.TryToRemove(paymentAmount, outstandingLoan.CurrencyType);
						//set the current principal to the new amount
						int amountOwed = outstandingLoan.AmountOwed;
						outstandingLoan.CurrentPrincipal = amountOwed - amountPaid;
						//set the day of the last payment to now so interest is calculated from this point forward
						outstandingLoan.DayLastPaymentMade = WorldClock.DaysSinceBeginningOfTime;
						//record how much we've paid in total
						outstandingLoan.TotalAmountPaid += paymentAmount;
						//see if we've paid off the loan at this point
						if (outstandingLoan.TotalAmountPaid >= outstandingLoan.AmountOwed) {
								outstandingLoan.HasBeenPaid = true;
								outstandingLoan.DayPaidOff = WorldClock.DaysSinceBeginningOfTime;
								GUIManager.PostSuccess("You have paid off your loan with " + outstandingLoan.OrganizationName);
						} else {
								outstandingLoan.HasBeenPaid = false;
						}
				}

				public int GetNumLoansFromOrganization(string organizationName)
				{
						int numLoans = 0;
						for (int i = 0; i < Loans.Count; i++) {
								if (Loans[i].OrganizationName.Equals(organizationName)) {
										numLoans++;
								}
						}
						return numLoans;
				}

				public bool HasOutstandingLoan(string organizationName)
				{
						for (int i = 0; i < Loans.Count; i++) {
								if (!Loans[i].HasBeenPaid && Loans[i].OrganizationName.Equals(organizationName)) {
										return true;
								}
						}
						return false;
				}

				public bool HasOutstandingLoan(string organizationName, out Loan outstandingLoan)
				{ 
						outstandingLoan = null;
						for (int i = 0; i < Loans.Count; i++) {
								if (!Loans[i].HasBeenPaid && Loans[i].OrganizationName.Equals(organizationName)) {
										outstandingLoan = Loans[i];
										outstandingLoan.CurrentDay = WorldClock.DaysSinceBeginningOfTime;
										break;
								}
						}
						return outstandingLoan != null;
				}
		}

		[Serializable]
		public class Loan : Mod
		{
				public bool HasBeenPaid = false;
				public WICurrencyType CurrencyType = WICurrencyType.D_Luminite;
				public string OrganizationName = "Moneylender";
				public int LoanNumber = 0;
				public int InitialPrincipal = 0;
				public int TotalAmountPaid = 0;

				public int AmountOwed {
						get {
								if (!HasBeenPaid) {
										if (DaysSinceLastPayment == 0) {
												return CurrentPrincipal;
										} else {
												return CurrentPrincipal + (int)Math.Floor(Math.Pow(CurrentPrincipal, DailyInterestRate * DaysSinceLastPayment));
										}
								}
								return 0;
						}
				}

				public int DaysSinceLastPayment {
						get {
								return CurrentDay - DayLastPaymentMade;
						}
				}

				public int CurrentPrincipal = 0;
				public int DayCreated = 0;
				public int CurrentDay = 0;
				public int DayLastPaymentMade = 0;
				public int DayPaidOff = 0;
				public double DailyInterestRate = 0;
				public List <string> CollateralReputation = new List <string>();
				public List <MobileReference> CollateralStructures = new List <MobileReference>();
		}
}