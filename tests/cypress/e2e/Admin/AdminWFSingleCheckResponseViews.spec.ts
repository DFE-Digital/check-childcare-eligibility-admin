const eligibilityCodePrefixes = {
  cannotBeUsedYet: "700",
  validForThisTerm: "701",
  validForThisTermAndNextTerm: "702",
  inGracePeriod: "703",
  expired: "704",
};

const temporaryCodeSuffix = "1";
const fosterCodeSuffix = "4";
const permanentCodeSuffix = "9";
const applyDvsdNinoPrefix = "NN";
const reconfirmationStatusDueNowNinoSuffix = "C";
const codeBody = "1234567";
const childDateOfBirth = {
  day: "1",
  month: "1",
  year: String(new Date().getFullYear() - 3),
};
const childTooYoungDateOfBirth = (() => {
  const date = new Date();
  date.setDate(1);
  date.setMonth(date.getMonth() - 5);

  return {
    day: "1",
    month: String(date.getMonth() + 1),
    year: String(date.getFullYear()),
  };
})();
const childTooOldDateOfBirth = {
  day: "1",
  month: "1",
  year: String(new Date().getFullYear() - 6),
};

function buildEligibilityCode(
  prefix: string,
  suffix = permanentCodeSuffix,
): string {
  return `${prefix}${codeBody}${suffix}`;
}

function runWorkingFamiliesCheck(
  code: string,
  nino = "AA123456B",
  dateOfBirth = childDateOfBirth,
): void {
  cy.visit("/home");
  cy.contains("a", "Run a check").click();
  cy.contains("button", "Childcare for working families").click();

  cy.get('[id="Child.EligibilityCode"]').type(code);
  cy.get("#NationalInsuranceNumber").type(nino);
  cy.get('[id="Child.ChildDateOfBirth.Day"]').type(dateOfBirth.day);
  cy.get('[id="Child.ChildDateOfBirth.Month"]').type(dateOfBirth.month);
  cy.get('[id="Child.ChildDateOfBirth.Year"]').type(dateOfBirth.year);
  cy.contains("button", "Run check").click();

  cy.get('[data-type="Response"]', { timeout: 35000 }).should("be.visible");
}

function assertTermValidityDetails(
  expectedText: string | undefined,
  expectedTermCount?: number,
): void {
  cy.get(".govuk-panel__body")
    .should("be.visible")
    .invoke("text")
    .should("not.be.empty")
    .then((text) => {
      if (expectedText !== undefined) {
        expect(text.trim()).to.contain(expectedText);
      }
      if (expectedTermCount !== undefined) {
        const terms = text.match(/\b(?:spring|summer|autumn) term\b/gi) ?? [];
        expect(terms).to.have.length(expectedTermCount);
      }
    });
}

function assertBannerExpiryDate(): void {
  cy.get(".govuk-panel__body")
    .invoke("text")
    .then((text) => {
      const dates = text.match(/\b\d{1,2} [A-Za-z]+ \d{4}\b/g) ?? [];
      expect(dates).to.have.length(1);
      expect(dates[0]).to.match(/^\d{1,2} [A-Za-z]+ \d{4}$/);
    });
}

function assertBannerReconfirmationMessage(
  expectedContent: string | RegExp,
): void {
  cy.get('[class~="govuk-body-l"][class~="govuk-!-margin-bottom-0"]')
    .invoke("text")
    .then((text) => {
      const content = text.trim();

      if (typeof expectedContent === "string") {
        expect(content).to.equal(expectedContent);
      } else {
        expect(content).to.match(expectedContent);
      }
    });
}

function formatDateOfBirth(dateOfBirth: {
  day: string;
  month: string;
  year: string;
}): string {
  const date = new Date(
    Number(dateOfBirth.year),
    Number(dateOfBirth.month) - 1,
    Number(dateOfBirth.day),
  );
  return date.toLocaleDateString("en-GB", {
    day: "numeric",
    month: "long",
    year: "numeric",
  });
}

function assertResponseDetails(
  code: string,
  nino: string,
  gped: string | undefined,
  reconfirmationLabel: string,
  reconfirmationDetails: string | undefined,
  reconfirmationStatus: string | undefined,
  dateOfBirth = childDateOfBirth,
): void {
  cy.contains("h2", "Details checked")
    .next("dl")
    .within(() => {
      cy.contains(".govuk-summary-list__key", "Eligibility code")
        .siblings(".govuk-summary-list__value")
        .invoke("text")
        .then((text) => expect(text.trim()).to.equal(code));
      cy.contains(".govuk-summary-list__key", "National Insurance number")
        .siblings(".govuk-summary-list__value")
        .invoke("text")
        .then((text) => expect(text.trim()).to.equal(nino));
      cy.contains(".govuk-summary-list__key", "Child date of birth")
        .siblings(".govuk-summary-list__value")
        .invoke("text")
        .then((text) =>
          expect(text.trim()).to.equal(formatDateOfBirth(dateOfBirth)),
        );
    });

  cy.contains("h2", "Code details")
    .next("dl")
    .within(() => {
      cy.contains(".govuk-summary-list__key", "Eligibility confirmed on")
        .siblings(".govuk-summary-list__value")
        .invoke("text")
        .should("match", /^\s*\d{1,2} [A-Za-z]+ \d{4}\s*$/);
      cy.contains(".govuk-summary-list__key", reconfirmationLabel)
        .siblings(".govuk-summary-list__value")
        .invoke("text")
        .then((text) => {
          const value = text.trim();
          if (code.endsWith(temporaryCodeSuffix)) {
            expect(value).to.match(/^\d{1,2} [A-Za-z]+ \d{4}$/);
          } else if (reconfirmationDetails !== undefined) {
            expect(value).to.equal(reconfirmationDetails);
          } else {
            expect(value).to.match(
              /^\d{1,2} [A-Za-z]+ \d{4} and \d{1,2} [A-Za-z]+ \d{4}$/,
            );
          }
        });
      cy.contains(".govuk-summary-list__key", "Grace period ends")
        .siblings(".govuk-summary-list__value")
        .invoke("text")
        .then((text) => {
          const value = text.trim();

          if (gped !== undefined) {
            expect(value).to.equal(gped);
          } else {
            expect(value).to.match(/^\d{1,2} [A-Za-z]+ \d{4}$/);
          }
        });
      cy.contains(".govuk-summary-list__key", "Reconfirmation status")
        .siblings(".govuk-summary-list__value")
        .invoke("text")
        .then((text) => {
          const value = text.trim();
          if (reconfirmationStatus !== undefined) {
            expect(value).to.equal(reconfirmationStatus);
          }
        });
    });
}

function assertValidityStartsBeforeChildIsNineMonthsOld(dateOfBirth: {
  day: string;
  month: string;
  year: string;
}): void {
  const childDate = new Date(
    Number(dateOfBirth.year),
    Number(dateOfBirth.month) - 1,
    Number(dateOfBirth.day),
  );
  const childNineMonthsDate = new Date(childDate);
  childNineMonthsDate.setMonth(childNineMonthsDate.getMonth() + 9);

  cy.contains(".govuk-summary-list__key", "Eligibility confirmed on")
    .siblings(".govuk-summary-list__value")
    .invoke("text")
    .then((text) => {
      const eligibilityConfirmedDate = new Date(text.trim());
      expect(eligibilityConfirmedDate.getTime()).to.be.lessThan(
        childNineMonthsDate.getTime(),
      );
    });
}

describe("Single check Working Families response views", () => {
  beforeEach(() => {
    cy.loginManchesterLA();
  });

  it("shows a code that cannot be used yet", () => {
    const code = buildEligibilityCode(eligibilityCodePrefixes.cannotBeUsedYet);
    runWorkingFamiliesCheck(code);

    cy.get(".govuk-panel__title").should(
      "contain.text",
      "Code cannot be used yet",
    );
    cy.get(".govuk-panel").should("have.class", "govuk-panel--blue");
    assertTermValidityDetails("Valid from", 1);
    assertResponseDetails(
      code,
      "AA123456B",
      "Date will appear here when the code can be used",
      "Reconfirm between",
      undefined,
      "Not due yet",
    );
  });

  it("shows a code valid for this term", () => {
    const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm);
    runWorkingFamiliesCheck(code);

    cy.get(".govuk-panel__title").should("contain.text", "Code valid");
    assertTermValidityDetails("Valid for", 1);
    assertResponseDetails(
      code,
      "AA123456B",
      undefined,
      "Reconfirm between",
      undefined,
      "Not due yet",
    );
  });

  it("shows a child too young response when the code starts before the child is nine months old", () => {
    const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm);
    runWorkingFamiliesCheck(code, "AA123456B", childTooYoungDateOfBirth);

    cy.get(".govuk-panel__title").should(
      "contain.text",
      "Code Child is too young",
    );
    cy.get(".govuk-panel").should("have.class", "govuk-panel--blue");
    assertTermValidityDetails("Valid from", 1);
    assertResponseDetails(
      code,
      "AA123456B",
      "Date will appear here when the code can be used",
      "Reconfirm between",
      undefined,
      "Not due yet",
      childTooYoungDateOfBirth,
    );
    assertValidityStartsBeforeChildIsNineMonthsOld(childTooYoungDateOfBirth);
  });

  it("shows a child too old reconfirmation status", () => {
    const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm);
    runWorkingFamiliesCheck(code, "AA123456B", childTooOldDateOfBirth);

    cy.get(".govuk-panel__title").should("contain.text", "Code expired");
    assertBannerReconfirmationMessage(
      "Child has reached compulsory school age",
    );
    cy.get(".govuk-tag").should("contain.text", "Child too old");
    assertResponseDetails(
      code,
      "AA123456B",
      undefined,
      "Reconfirm between",
      "Not applicable",
      "Child too old",
      childTooOldDateOfBirth,
    );
    cy.contains(".govuk-summary-list__key", "Reconfirm between")
      .siblings(".govuk-summary-list__value")
      .should("contain.text", "Not applicable");
  });

  it("shows a code valid for this term and next term", () => {
    const code = buildEligibilityCode(
      eligibilityCodePrefixes.validForThisTermAndNextTerm,
    );
    runWorkingFamiliesCheck(code);

    cy.get(".govuk-panel__title").should("contain.text", "Code valid");
    assertTermValidityDetails("Valid for", 2);
    assertResponseDetails(
      code,
      "AA123456B",
      undefined,
      "Reconfirm between",
      undefined,
      "Not due yet",
    );
  });

  it("shows a standard code in its grace period", () => {
    const code = buildEligibilityCode(eligibilityCodePrefixes.inGracePeriod);
    runWorkingFamiliesCheck(code);

    cy.get(".govuk-panel__title").should(
      "contain.text",
      "Code in grace period",
    );
    cy.get(".govuk-panel").should("have.class", "govuk-panel--yellow");
    assertTermValidityDetails("Expires on");
    assertBannerExpiryDate();
    assertBannerReconfirmationMessage("Needs reconfirming now");
    assertResponseDetails(
      code,
      "AA123456B",
      undefined,
      "Reconfirm between",
      undefined,
      "Overdue",
    );
  });

  it("shows a temporary code in its grace period", () => {
    const code = buildEligibilityCode(
      eligibilityCodePrefixes.inGracePeriod,
      temporaryCodeSuffix,
    );
    runWorkingFamiliesCheck(code);

    cy.get(".govuk-panel__title").should(
      "contain.text",
      "Temporary code in grace period",
    );
    cy.get(".govuk-panel").should("have.class", "govuk-panel--yellow");
    assertTermValidityDetails("Expires on");
    assertBannerExpiryDate();
    assertResponseDetails(
      code,
      "AA123456B",
      undefined,
      "Apply for a new code by",
      undefined,
      "Not applicable",
    );
  });

  it("shows an expired code", () => {
    const code = buildEligibilityCode(eligibilityCodePrefixes.expired);
    runWorkingFamiliesCheck(code);

    cy.get(".govuk-panel__title").should("contain.text", "Code expired");
    cy.get(".govuk-panel").should("have.class", "govuk-panel--orange");
    assertTermValidityDetails("Expired on");
    assertBannerExpiryDate();
    assertResponseDetails(
      code,
      "AA123456B",
      undefined,
      "Reconfirm between",
      undefined,
      "Overdue",
    );
  });

  it("shows a temporary code is valid", () => {
    const code = buildEligibilityCode(
      eligibilityCodePrefixes.validForThisTerm,
      temporaryCodeSuffix,
    );
    runWorkingFamiliesCheck(code);

    cy.get(".govuk-panel__title").should(
      "contain.text",
      "Temporary code valid",
    );
    cy.contains("Apply for a new code by").should("exist");
    assertTermValidityDetails(undefined, 1);
    assertResponseDetails(
      code,
      "AA123456B",
      undefined,
      "Apply for a new code by",
      undefined,
      "Not applicable",
    );
  });

  it("shows a foster family code", () => {
    const code = buildEligibilityCode(
      eligibilityCodePrefixes.validForThisTerm,
      fosterCodeSuffix,
    );
    runWorkingFamiliesCheck(code);

    cy.get(".govuk-panel__title").should(
      "contain.text",
      "Foster family code valid",
    );
    assertTermValidityDetails("Valid for", 1);

    assertResponseDetails(
      code,
      "AA123456B",
      undefined,
      "Reconfirm between",
      undefined,
      "Not due yet",
    );
  });

  it("shows the due now reconfirmation status", () => {
    const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm);
    const nino = `AB123456${reconfirmationStatusDueNowNinoSuffix}`;
    runWorkingFamiliesCheck(code, nino);

    cy.get(".govuk-tag").should("contain.text", "Due now");
    assertBannerReconfirmationMessage(
      /^Needs reconfirming before \d{1,2} [A-Za-z]+ \d{4}$/,
    );
    assertTermValidityDetails(undefined, 1);
    assertResponseDetails(
      code,
      nino,
      undefined,
      "Reconfirm between",
      undefined,
      "Due now",
    );
  });
});
