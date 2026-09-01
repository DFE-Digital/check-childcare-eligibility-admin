//Test set as SKIP due to the length of time a tech error response takes to return, so we do not want to
//  include this in regular test runs. The timeout is set to how long it will keep checking rather than a 
// fixed wait, but our intention is to see if we can generate the response faster before including
// this test more permanently. Then just remove SKIP from filename to re-enable it.

describe('TechnicalError outcome should display Error Code and CorrelationID', () => {
    const parentLastName = 'Tester';

    it('TechnicalError outcome should display Error Code and CorrelationID', () => {
        // Login with LA session
        cy.checkSession('LA');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home")
        cy.wait(1);
        cy.get('h1').should('include.text', 'Manage eligibility for childcare support');
        
        // Navigate to run a check
        cy.contains('Run a check').click();

        // Select 2YO eligibility type (Working Families does not include error code or correlation ID at present)
        cy.get('h1').should('include.text', 'Run a check for one parent or guardian');
        cy.contains('button', 'Early learning for 2-year-olds').click();

        // Consent declaration (if exists in the flow)
        cy.url().should('include', '/Check/Enter_Details');

        // Add parent details
        cy.get('#LastName').type(parentLastName);
        cy.get('[id="DateOfBirth.Day"]').type('01');
        cy.get('[id="DateOfBirth.Month"]').type('01');
        cy.get('[id="DateOfBirth.Year"]').type('1990');
        cy.get('#NationalInsuranceNumber').type("XX123456C");
        cy.contains('button', 'Run check').click();

        // Loader page
        cy.url().should('include', 'Check/Loader');

        // Technical_Error outcome page
        cy.get('h1',{ timeout: 120000 }).should('include.text', 'Check failed');
        cy.get('body').should('include.text', 'Error code: STE50');
        cy.get('body').should('include.text', 'Correlation ID:'); //Only shown if Guid was available from the check
    });
});