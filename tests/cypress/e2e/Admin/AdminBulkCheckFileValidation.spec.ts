import "cypress-file-upload";

describe("Admin Bulk Check File Validation Journey", () => {
  beforeEach(() => {
    cy.checkSession("LA");
    cy.visit(Cypress.config().baseUrl ?? "");
    cy.contains("Run a batch check").click();
    cy.contains("button", "Early learning for 2-year-olds").click();
  });

  it("will return an error message if the bulk file contains more than 250 rows of data", () => {
    cy.fixture("BulkcheckFileValidaiton/bulkchecktemplate_too_many_records.csv").then(
      (fileContent1) => {
        cy.get('input[type="file"]').attachFile([
          {
            fileContent: fileContent1,
            fileName: "bulkchecktemplate_too_many_records.csv",
            mimeType: "text/csv",
          },
        ]);
      }
    );
    cy.contains("Run check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "The selected file must contain fewer than 250 rows"
      );
    });
  });

  it("will return an error message headers are incorrect", () => {
    cy.fixture("BulkcheckFileValidaiton/bulkchecktemplate_wrong_headers.csv").then(
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

  it("will return an error message if file size is greater than 10MB", () => {
    cy.fixture("BulkcheckFileValidaiton/bulkchecktemplate_too_large.csv").then(
      (fileContent1) => {
        cy.get('input[type="file"]').attachFile([
          {
            fileContent: fileContent1,
            fileName: "bulkchecktemplate_too_large.csv",
            mimeType: "text/csv",
          },
        ]);
      }
    );
    cy.contains("Run check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "The selected file must be smaller than 10MB"
      );
    });
  });

    it("will return an error message if file is empty", () => {
    cy.fixture("BulkcheckFileValidaiton/bulkchecktemplate_empty.csv").then(
      (fileContent1) => {
        cy.get('input[type="file"]').attachFile([
          {
            fileContent: fileContent1,
            fileName: "bulkchecktemplate_empty.csv",
            mimeType: "text/csv",
          },
        ]);
      }
    );
    cy.contains("Run check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "The selected file is empty"
      );
    });
  });

  it("will return an error message if file is the wrong format", () => {
    cy.fixture("BulkcheckFileValidaiton/bulkchecktemplate_wrong_format.txt").then(
      (fileContent1) => {
        cy.get('input[type="file"]').attachFile([
          {
            fileContent: fileContent1,
            fileName: "bulkchecktemplate_wrong_format.txt",
            mimeType: "text/plain",
          },
        ]);
      }
    );
    cy.contains("Run check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "The selected file must be a CSV"
      );
    });
  });

  it("will return an error message if not file is selected", () => {
    cy.contains("Run check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "Select a CSV file"
      );
    });
  });

  it("will return an error message if more than 10 batches are attempted within an hour", () => {
    for (let i = 0; i < 11; i++) {
      cy.fixture("BulkcheckFileValidaiton/bulkchecktemplate_too_many_records.csv").then(
        (fileContent1) => {
          cy.get('input[type="file"]').attachFile([
            {
              fileContent: fileContent1,
              fileName: "bulkchecktemplate_too_many_records.csv",
              mimeType: "text/csv",
            },
          ]);
        }
      );
      cy.contains("Run check").click();
    }
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "No more than 10 batch check requests can be made per hour"
      );
    });
  });

  it("will run a successful batch check and redirect to bulk check status page", () => {
    cy.fixture("BulkcheckFileValidaiton/bulkchecktemplate_complete.csv").then((fileContent1) => {
      cy.get('input[type="file"]').attachFile([
        {
          fileContent: fileContent1,
          fileName: "bulkchecktemplate_complete.csv",
          mimeType: "text/csv",
        },
      ]);
    });
    cy.contains("Run check").click();
    cy.get("h2", { timeout: 80000 }).should(
      "include.text",
      "Batch checks status"
    );
    cy.get("td").should("include.text", "bulkchecktemplate_complete.csv");
  });
});
