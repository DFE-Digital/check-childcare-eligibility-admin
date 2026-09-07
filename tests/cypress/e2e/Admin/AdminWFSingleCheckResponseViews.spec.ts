const eligibilityCodePrefixes = {
    cannotBeUsedYet: '700',
    validForThisTerm: '701',
    validForThisTermAndNextTerm: '702',
    inGracePeriod: '703',
    expired: '704'
};

const temporaryCodeSuffix = '1';
const fosterCodeSuffix = '4';
const permanentCodeSuffix = '9';
const applyDvsdNinoPrefix = 'NN';
const reconfirmationStatusDueNowNinoSuffix = 'C';
const codeBody = '1234567';
const childDateOfBirth = {
    day: '1',
    month: '1',
    year: String(new Date().getFullYear() - 3)
};
const childTooYoungDateOfBirth = (() => {
    const date = new Date();
    date.setDate(1);
    date.setMonth(date.getMonth() - 5);

    return {
        day: '1',
        month: String(date.getMonth() + 1),
        year: String(date.getFullYear())
    };
})();

function buildEligibilityCode(prefix: string, suffix = permanentCodeSuffix): string {
    return `${prefix}${codeBody}${suffix}`;
}

function runWorkingFamiliesCheck(
    code: string,
    nino = 'AA123456B',
    dateOfBirth = childDateOfBirth
): void {
    cy.visit('/home');
    cy.contains('a', 'Run a check').click();
    cy.contains('button', 'Childcare for working families').click();

    cy.get('#Child_EligibilityCode').type(code);
    cy.get('#NationalInsuranceNumber').type(nino);
    cy.get('#Day').type(dateOfBirth.day);
    cy.get('#Month').type(dateOfBirth.month);
    cy.get('#Year').type(dateOfBirth.year);
    cy.contains('button', 'Run check').click();

    cy.get('[data-type="Response"]', { timeout: 35000 }).should('be.visible');
}

function assertTermValidityDetails(expectedText: string | undefined, expectedTermCount?: number): void {
    cy.get('.govuk-panel__body')
        .should('be.visible')
        .invoke('text')
        .should('not.be.empty')
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

function assertSingleValidityDate(): void {
    cy.get('.govuk-panel__body')
        .invoke('text')
        .then((text) => {
            const dates = text.match(/\b\d{1,2} [A-Za-z]+ \d{4}\b/g) ?? [];
            expect(dates).to.have.length(1);
            expect(dates[0]).to.match(/^\d{1,2} [A-Za-z]+ \d{4}$/);
        });
}

    function formatDateOfBirth(dateOfBirth: { day: string; month: string; year: string }): string {
        const date = new Date(Number(dateOfBirth.year), Number(dateOfBirth.month) - 1, Number(dateOfBirth.day));
        return date.toLocaleDateString('en-GB', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    }

    function assertResponseDetails(
        code: string,
        nino: string,
        reconfirmationLabel: string,
        dateOfBirth = childDateOfBirth
    ): void {
        cy.contains('h2', 'Details checked').next('dl').within(() => {
            cy.contains('.govuk-summary-list__key', 'Eligibility code')
                .siblings('.govuk-summary-list__value')
                .should('have.text', code);
            cy.contains('.govuk-summary-list__key', 'National Insurance number')
                .siblings('.govuk-summary-list__value')
                .should('have.text', nino);
            cy.contains('.govuk-summary-list__key', 'Child date of birth')
                .siblings('.govuk-summary-list__value')
                .should('have.text', formatDateOfBirth(dateOfBirth));
        });

        cy.contains('h2', 'Code details').next('dl').within(() => {
            cy.contains('.govuk-summary-list__key', 'Eligibility confirmed on')
                .siblings('.govuk-summary-list__value')
                .invoke('text')
                .should('match', /^\s*\d{1,2} [A-Za-z]+ \d{4}\s*$/);
            cy.contains('.govuk-summary-list__key', reconfirmationLabel)
                .siblings('.govuk-summary-list__value')
                .should('not.be.empty');
            cy.contains('.govuk-summary-list__key', 'Grace period ends')
                .siblings('.govuk-summary-list__value')
                .should('not.be.empty');
            cy.contains('.govuk-summary-list__key', 'Reconfirmation status')
                .siblings('.govuk-summary-list__value')
                .find('.govuk-tag')
                .should('not.be.empty');
        });
    }

function assertValidityStartsBeforeChildIsNineMonthsOld(dateOfBirth: { day: string; month: string; year: string }): void {
    const childDate = new Date(Number(dateOfBirth.year), Number(dateOfBirth.month) - 1, Number(dateOfBirth.day));
    const childNineMonthsDate = new Date(childDate);
    childNineMonthsDate.setMonth(childNineMonthsDate.getMonth() + 9);

    cy.contains('.govuk-summary-list__key', 'Eligibility confirmed on')
        .siblings('.govuk-summary-list__value')
        .invoke('text')
        .then((text) => {
            const eligibilityConfirmedDate = new Date(text.trim());
            expect(eligibilityConfirmedDate.getTime()).to.be.lessThan(childNineMonthsDate.getTime());
        });
}

describe('Single check Working Families response views', () => {
    beforeEach(() => {
        cy.loginManchesterLA();
    });

    it('shows a code that cannot be used yet', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.cannotBeUsedYet);
        runWorkingFamiliesCheck(code);

        cy.get('.govuk-panel__title').should('contain.text', 'Code cannot be used yet');
        cy.get('.govuk-panel').should('have.class', 'govuk-panel--blue');
        assertTermValidityDetails('Valid from', 1);
        assertResponseDetails(code, 'AA123456B', 'Reconfirm between');
    });

    it('shows a code valid for this term', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm);
        runWorkingFamiliesCheck(code);

        cy.get('.govuk-panel__title').should('contain.text', 'Code valid');
        cy.get('.govuk-panel').should('have.class', 'govuk-panel--confirmation');
        assertTermValidityDetails('Valid for', 1);
        assertResponseDetails(code, 'AA123456B', 'Reconfirm between');
    });

    it('shows a child too young response when the code starts before the child is nine months old', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm);
        runWorkingFamiliesCheck(code, 'AA123456B', childTooYoungDateOfBirth);

        cy.get('.govuk-panel__title').should('contain.text', 'Code Child is too young');
        cy.get('.govuk-panel').should('have.class', 'govuk-panel--blue');
        assertTermValidityDetails('Valid from', 1);
        assertResponseDetails(code, 'AA123456B', 'Reconfirm between', childTooYoungDateOfBirth);
        assertValidityStartsBeforeChildIsNineMonthsOld(childTooYoungDateOfBirth);
    });

    it('shows a code valid for this term and next term', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTermAndNextTerm);
        runWorkingFamiliesCheck(code);

        cy.get('.govuk-panel__title').should('contain.text', 'Code valid');
        cy.get('.govuk-panel').should('have.class', 'govuk-panel--confirmation');
        assertTermValidityDetails('Valid for', 2);
        assertResponseDetails(code, 'AA123456B', 'Reconfirm between');
    });

    it('shows a code in its grace period', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.inGracePeriod);
        runWorkingFamiliesCheck(code);

        cy.get('.govuk-panel__title').should('contain.text', 'Code in grace period');
        cy.get('.govuk-panel').should('have.class', 'govuk-panel--yellow');
        assertTermValidityDetails('Expires on');
        assertSingleValidityDate();
        assertResponseDetails(code, 'AA123456B', 'Reconfirm between');
    });

    it('shows an expired code', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.expired);
        runWorkingFamiliesCheck(code);

        cy.get('.govuk-panel__title').should('contain.text', 'Code expired');
        cy.get('.govuk-panel').should('have.class', 'govuk-panel--orange');
        assertTermValidityDetails('Expired on');
        assertSingleValidityDate();
        assertResponseDetails(code, 'AA123456B', 'Reconfirm between');
    });

    it('shows a temporary code', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm, temporaryCodeSuffix);
        runWorkingFamiliesCheck(code);

        cy.get('.govuk-panel__title').should('contain.text', 'Temporary code valid');
        cy.contains('Apply for a new code by').should('exist');
        assertTermValidityDetails(undefined, 1);
        assertResponseDetails(code, 'AA123456B', 'Apply for a new code by');
    });

    it('shows a foster family code', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm, fosterCodeSuffix);
        runWorkingFamiliesCheck(code);

        cy.get('.govuk-panel__title').should('contain.text', 'Foster family code valid');
        assertTermValidityDetails('Valid for', 1);
        assertResponseDetails(code, 'AA123456B', 'Reconfirm between');
    });

    it('shows the due now reconfirmation status', () => {
        const code = buildEligibilityCode(eligibilityCodePrefixes.validForThisTerm);
        const nino = `${applyDvsdNinoPrefix}123456${reconfirmationStatusDueNowNinoSuffix}`;
        runWorkingFamiliesCheck(
            code,
            nino
        );

        cy.get('.govuk-tag').should('contain.text', 'Due now');
        cy.get('.govuk-panel').should('contain.text', 'Needs reconfirming before');
        assertTermValidityDetails(undefined, 1);
        assertResponseDetails(code, nino, 'Reconfirm between');
    });
});