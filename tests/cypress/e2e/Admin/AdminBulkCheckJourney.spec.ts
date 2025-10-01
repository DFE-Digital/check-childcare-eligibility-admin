import 'cypress-file-upload';

describe('Admin Bulk Check Journey', () => {
    beforeEach(() => {
        cy.checkSession('LA');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.contains('Run a batch check').click();
        cy.contains('button', 'Early learning for 2-year-olds').click();
    });

    it("will return an error message if the bulk file contains more than 250 rows of data", () => {
        cy.fixture('bulkchecktemplate_too_many_records.csv').then(fileContent1 => {
            cy.get('input[type="file"]').attachFile([
                {
                    fileContent: fileContent1,
                    fileName: 'bulkchecktemplate_too_many_records.csv',
                    mimeType: 'text/csv'
                }
            ]);            
        });
        cy.contains('Run check').click();
        cy.get('#file-upload-1-error').as('errorMessage');
        cy.get('@errorMessage').should(($p) => {
            expect($p.first()).to.contain('The selected file must contain fewer than 250 rows');
        });
    });

    it("will return an error message if more than 10 batches are attempted within an hour", () => {
        for (let i = 0; i < 11; i++) {
            cy.fixture('bulkchecktemplate_too_many_records.csv').then(fileContent1 => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: 'bulkchecktemplate_too_many_records.csv',
                        mimeType: 'text/csv'
                    }
                ]);            
            });
           cy.contains('Run check').click();
        }
        cy.get('#file-upload-1-error').as('errorMessage');
        cy.get('@errorMessage').should(($p) => {
            expect($p.first()).to.contain('No more than 10 batch check requests can be made per hour');
        });
    });

    /* it("will run a successful batch check", () => {
        cy.fixture('bulkchecktemplate_complete.csv').then(fileContent1 => {
            cy.get('input[type="file"]').attachFile([
                {
                    fileContent: fileContent1,
                    fileName: 'bulkchecktemplate_complete.csv',
                    mimeType: 'text/csv'
                }
            ]);            
        });
        cy.contains('Run check').click();
        cy.get('h1', { timeout: 80000 }).should('include.text', 'Checks completed');
        cy.contains("Download").click();
    }); */
});
