import "cypress-file-upload";

describe("Admin Bulk Check File Validation Journey", () => {
  beforeEach(() => {
    cy.checkSession("LA");
    cy.visit((Cypress.config().baseUrl ?? "") + "/home")
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
  
  it("will download outcome CSV with columns matching template order", () => {
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
  
    cy.get("h1.govuk-heading-l", { timeout: 80000 }).should(
      "include.text",
      "Batch checks status"
    );
  
    cy.get("td").should("include.text", "bulkchecktemplate_complete.csv");
  
    const waitForDownloadResults = (attempt = 0) => {
      if (attempt >= 6) {
        throw new Error("Download results link did not appear after 30 seconds");
      }
    
      cy.get("body").then(($body) => {
        if ($body.find("a:contains('Download results')").length > 0) {
          cy.contains("a", "Download results").then(($a) => {
            const href = $a.attr("href") ?? "";
    
            const downloadUrl = href.startsWith("http")
              ? href
              : `${Cypress.config().baseUrl}${href}`;
    
            cy.request(downloadUrl).then((response) => {
              expect(response.status).to.eq(200);
    
              const firstLine = response.body.split(/\r?\n/)[0];
    
              expect(firstLine).to.eq(
                "Parent Last Name,Parent Date of Birth,Parent National Insurance number,Outcome"
              );
            });
          });
        } else {
          cy.wait(5000);
          cy.reload();
          waitForDownloadResults(attempt + 1);
        }
      });
    };

  });
  it("will run a successful batch check when last name contains a curly apostrophe", () => {
    cy.fixture("BulkcheckFileValidaiton/bulkchecktemplate_curly_apostrophe.csv").then((fileContent1) => {
      cy.get('input[type="file"]').attachFile([
        {
          fileContent: fileContent1,
          fileName: "bulkchecktemplate_curly_apostrophe.csv",
          mimeType: "text/csv",
        },
      ]);
    });
  
    cy.contains("Run check").click();
  
    cy.get("h1.govuk-heading-l", { timeout: 80000 }).should(
      "include.text",
      "Batch checks status"
    );
  
    cy.get("td").should(
      "include.text",
      "bulkchecktemplate_curly_apostrophe.csv"
    );
  });

  it("shows a confirmation page before deleting a batch file", () => {
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
  
    cy.get("h1.govuk-heading-l", { timeout: 80000 }).should(
      "include.text",
      "Batch checks status"
    );
  
    cy.contains("tr", "bulkchecktemplate_complete.csv")
      .contains("Delete")
      .click();
  
    cy.get("h1.govuk-heading-l").should(
      "include.text",
      "Delete batch file bulkchecktemplate_complete.csv?"
    );
  
    cy.contains(
      "You will no longer be able to view or download results."
    ).should("be.visible");
  
    cy.contains("button", "Delete file").click();
  
    cy.get("h1.govuk-heading-l").should("include.text", "Batch checks status");
    cy.contains("File deleted").should("be.visible");
    cy.contains("File deleted").should("be.visible");
  });

});
