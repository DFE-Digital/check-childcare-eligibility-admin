import "cypress-file-upload";

describe("Admin Working Families Bulk Check Content Validation Journey", () => {
  beforeEach(() => {
    cy.checkSession("LA");
    cy.visit(Cypress.config().baseUrl ?? "");
    cy.contains("Run a batch check").click();
    cy.contains("button", "Childcare for working families").click();
  });

 it("will return an error message headers are incorrect", () => {
    cy.fixture("BulkCheckContentValidation/WorkingFamilies/bulkchecktemplate_wrong_headers.csv").then(
      (fileContent1) => {
        cy.get('input[type="file"]').attachFile([
          {
            fileContent: fileContent1,
            fileName: "bulkchecktemplate_wrong_headers.csv",
            mimeType: "text/csv",
          },
        ]);
      }
    );
    cy.contains("Run check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "The column headings in the selected file must exactly match the template"
      );
    });
  });
  it("will return error page with correct headings and warnings", () => {
    cy.fixture("BulkCheckContentValidation/WorkingFamilies/bulkchecktemplate_invalid_inputs.csv").then(
      (fileContent1) => {
        cy.get('input[type="file"]').attachFile([
          {
            fileContent: fileContent1,
            fileName: "bulkchecktemplate_invalid_inputs.csv",
            mimeType: "text/csv",
          },
        ]);
      }
    );
    cy.contains("Run check").click();
    cy.get("h1.govuk-heading-l", { timeout: 10000 }).should("contain.text", "Fix data errors");
    cy.get(".govuk-warning-text .govuk-warning-text__text").should(
      "contain.text",
      "We cannot run checks as the file you uploaded contains data errors."
    );
  });
  it("will return error page with content validation messages", () => {
    cy.fixture("BulkCheckContentValidation/WorkingFamilies/bulkchecktemplate_invalid_inputs.csv").then(
      (fileContent1) => {
        cy.get('input[type="file"]').attachFile([
          {
            fileContent: fileContent1,
            fileName: "bulkchecktemplate_invalid_inputs.csv",
            mimeType: "text/csv",
          },
        ]);
      }
    );
    
    const validationErrors = [
  { line: "2", error: "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C" },
  { line: "4", error: "Eligibility code must only contain numbers" },
  { line: "5", error: "The date of birth must be in yyyy-mm-dd or dd-mm-yyyy format" },
  { line: "6", error: "Eligibility code must be 11 digits long" },
  { line: "6", error: "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C" },
  { line: "6", error: "The date of birth must be in yyyy-mm-dd or dd-mm-yyyy format" },
  { line: "7", error: "Enter an eligibility code that is 11 digits long" },
  { line: "8", error: "Enter parent or guardian's National Insurance number" },
  { line: "9", error: "Enter parent or guardian's date of birth" }
];
  cy.contains("Run check").click();
  cy.get("tbody.govuk-table__body tr.govuk-table__row", {timeout : 8000 }).should("have.length", validationErrors.length);

  validationErrors.forEach((expected, index) => {
    cy.get("tbody.govuk-table__body tr.govuk-table__row").eq(index).within(() => {
    cy.get("td.govuk-table__cell").eq(0).should("have.text", expected.line);
    cy.get("td.govuk-table__cell").eq(1).should("contain.text", expected.error);
  });
 });
});
  
it("will run a successful batch check and redirect to bulk check status page", () => {
    cy.fixture("BulkCheckContentValidation/WorkingFamilies/bulkchecktemplate_valid.csv").then((fileContent1) => {
      cy.get('input[type="file"]').attachFile([
        {
          fileContent: fileContent1,
          fileName: "bulkchecktemplate_valid.csv",
          mimeType: "text/csv",
        },
      ]);
    });
    cy.contains("Run check").click();
    cy.get("h1.govuk-heading-l", { timeout: 80000 }).should(
      "include.text",
      "Batch checks status"
    );
    cy.get("td").should("include.text", "bulkchecktemplate_valid.csv");
  });
});
